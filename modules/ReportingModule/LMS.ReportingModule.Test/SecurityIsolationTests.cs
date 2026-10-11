using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using LMS.ReportingModule;
using IY.Reporting.Models;
using IY.Reporting.Services;

namespace LMS.ReportingModule.Test;

public class SecurityIsolationTests
{
    private readonly string _solutionRoot;

    public SecurityIsolationTests()
    {
        var current = AppContext.BaseDirectory;
        var dir = new DirectoryInfo(current);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "LMS.sln")))
        {
            dir = dir.Parent;
        }
        _solutionRoot = dir?.FullName ?? @"D:\LMS";
    }

    #region Release Mode Tests

    [Fact]
    public void ReleaseCompilation_Metadata_ContainsNoProofTypesOrRoute()
    {
        var releaseDll = Path.Combine(_solutionRoot, "modules", "ReportingModule", "LMS.ReportingModule", "bin", "Release", "net9.0", "LMS.ReportingModule.dll");
        Assert.True(File.Exists(releaseDll), $"Release DLL must exist at {releaseDll}");

        var assembly = Assembly.Load(File.ReadAllBytes(releaseDll));
        var types = assembly.GetTypes();

        // 1. Proof services absent
        Assert.DoesNotContain(types, t => t.Name == "ProofReportExecutionContext");
        Assert.DoesNotContain(types, t => t.Name == "ProofReportAccessPolicy");
        Assert.DoesNotContain(types, t => t.Name == "ProofReportDataScopeProvider");
        Assert.DoesNotContain(types, t => t.Name == "ProofQueryExecutor");

        // 2. IntegrationProof route / component absent
        Assert.DoesNotContain(types, t => t.Name == "IntegrationProof");

        // 3. No type name containing Proof
        Assert.DoesNotContain(types, t => t.Name.Contains("Proof", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ReleaseModule_RegisterServices_DoesNotRegisterProofServices_EvenWithExplicitOptIn()
    {
        var releaseDll = Path.Combine(_solutionRoot, "modules", "ReportingModule", "LMS.ReportingModule", "bin", "Release", "net9.0", "LMS.ReportingModule.dll");
        Assert.True(File.Exists(releaseDll), $"Release DLL must exist at {releaseDll}");

        var assembly = Assembly.Load(File.ReadAllBytes(releaseDll));
        var moduleType = assembly.GetType("LMS.ReportingModule.Module");
        Assert.NotNull(moduleType);

        var module = (Shared.Core.Contracts.IModule)Activator.CreateInstance(moduleType)!;

        // Configure with explicit proof opt-in
        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "Reporting:EnableIntegrationProof", "true" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var services = new ServiceCollection();

        // Act
        module.RegisterServices(services, config);

        // Assert: Proof services MUST NOT be registered in Release module under any circumstances
        Assert.False(services.Any(sd => sd.ServiceType == typeof(IReportExecutionContext) && sd.ImplementationType?.Name.Contains("Proof") == true),
            "Proof IReportExecutionContext must not be registered in Release module");
        Assert.False(services.Any(sd => sd.ServiceType == typeof(IReportDataScopeProvider) && sd.ImplementationType?.Name.Contains("Proof") == true),
            "Proof IReportDataScopeProvider must not be registered in Release module");
        Assert.False(services.Any(sd => sd.ServiceType == typeof(IReportAccessPolicy) && sd.ImplementationType?.Name.Contains("Proof") == true),
            "Proof IReportAccessPolicy must not be registered in Release module");
        Assert.False(services.Any(sd => sd.ImplementationType?.Name.Contains("Proof") == true),
            "No proof implementation type may be registered in Release module");
    }

    [Fact]
    public void ReleaseModule_ProductionReporting_RemainsFailClosed_WithoutAdapters()
    {
        var releaseDll = Path.Combine(_solutionRoot, "modules", "ReportingModule", "LMS.ReportingModule", "bin", "Release", "net9.0", "LMS.ReportingModule.dll");
        var assembly = Assembly.Load(File.ReadAllBytes(releaseDll));
        var moduleType = assembly.GetType("LMS.ReportingModule.Module")!;
        var module = (Shared.Core.Contracts.IModule)Activator.CreateInstance(moduleType)!;

        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        module.RegisterServices(services, config);

        var provider = services.BuildServiceProvider();

        // Production adapter refuses an empty/unauthenticated service provider.
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IReportExecutionContext>());
    }

    #endregion

    #region Debug Mode Tests

#if DEBUG
    [Fact]
    public void DebugModule_WhenProofDisabled_DoesNotRegisterProofServices()
    {
        var services = new ServiceCollection();
        // Proof setting is FALSE (disabled)
        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "Reporting:EnableIntegrationProof", "false" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var module = new Module();

        // Act
        module.RegisterServices(services, configuration);

        // Assert
        Assert.False(services.Any(sd => sd.ServiceType == typeof(IReportExecutionContext) && sd.ImplementationType == typeof(ProofReportExecutionContext)),
            "Proof context must NOT be registered when Reporting:EnableIntegrationProof is false.");
        Assert.False(services.Any(sd => sd.ImplementationType == typeof(ProofReportExecutionContext)),
            "ProofReportExecutionContext must not be registered.");
        Assert.False(services.Any(sd => sd.ImplementationType == typeof(ProofReportAccessPolicy)),
            "ProofReportAccessPolicy must not be registered.");
        Assert.False(services.Any(sd => sd.ImplementationType == typeof(ProofReportDataScopeProvider)),
            "ProofReportDataScopeProvider must not be registered.");
        Assert.False(services.Any(sd => sd.ImplementationType == typeof(ProofQueryExecutor)),
            "ProofQueryExecutor must not be registered.");
    }

    [Fact]
    public void DebugModule_WhenProofExplicitlyEnabled_RegistersSyntheticHarness()
    {
        var services = new ServiceCollection();
        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "Reporting:EnableIntegrationProof", "true" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var module = new Module();

        // Act
        module.RegisterServices(services, configuration);

        // Assert
        Assert.True(services.Any(sd => sd.ImplementationType == typeof(ProofReportExecutionContext)),
            "ProofReportExecutionContext should be registered when proof is enabled in Debug.");
        Assert.True(services.Any(sd => sd.ImplementationType == typeof(ProofReportAccessPolicy)),
            "ProofReportAccessPolicy should be registered when proof is enabled in Debug.");
        Assert.True(services.Any(sd => sd.ImplementationType == typeof(ProofReportDataScopeProvider)),
            "ProofReportDataScopeProvider should be registered when proof is enabled in Debug.");
        Assert.True(services.Any(sd => sd.ImplementationType == typeof(ProofQueryExecutor)),
            "ProofQueryExecutor should be registered when proof is enabled in Debug.");
    }

    [Fact]
    public void DebugModule_ProofIdentity_IsSyntheticAndNonPrivileged()
    {
        var context = new ProofReportExecutionContext();

        // Must NOT be a real user
        Assert.Equal("synthetic-proof-runner-unprivileged", context.SubjectId);

        // Claims must contain NO admin/privileged roles
        Assert.False(context.Claims.ContainsKey("Admin"));
        Assert.False(context.Claims.ContainsKey("SuperAdmin"));
        Assert.Equal("None", context.Claims["Privilege"]);
        Assert.Equal("NonPrivilegedProofRunner", context.Claims["Role"]);
        Assert.Equal("integration-proof-only", context.Claims["Scope"]);
    }

    [Fact]
    public async Task DebugModule_ProofAccessPolicy_IsFailClosedDenyByDefault()
    {
        var policy = new ProofReportAccessPolicy();
        var context = new ProofReportExecutionContext();

        var request = (ReportAccessRequest)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ReportAccessRequest));
        var result = await policy.IsAuthorizedAsync(context, request, CancellationToken.None);

        // Deny-by-default: always false
        Assert.False(result, "ProofReportAccessPolicy must fail-closed (return false).");
    }

    [Fact]
    public async Task DebugModule_ProofQueryExecutor_IsSyntheticInMemoryOnly()
    {
        var executor = new ProofQueryExecutor();
        var context = new ProofReportExecutionContext();

        // Calling ExecuteCoreAsync returns synthetic in-memory result with no database connection
        var method = typeof(ProofQueryExecutor).GetMethod("ExecuteCoreAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        var task = (Task<ReportResult>)method.Invoke(executor, new object?[] { null, context, CancellationToken.None })!;
        var result = await task;

        Assert.NotNull(result);
        Assert.Empty(result.Columns);
        Assert.Empty(result.Rows);
    }

    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "LMS.Host";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    [Theory]
    [InlineData("Development", false, false)]
    [InlineData("Development", true, true)]
    [InlineData("Production", false, false)]
    [InlineData("Production", true, false)] // Mandatory reproduction case: Debug build under Production MUST NOT register proof services
    public void DebugModule_RealRegisterServices_EnforcesHostEnvironmentAndOptIn(string environmentName, bool optIn, bool shouldRegister)
    {
        var services = new ServiceCollection();
        var hostEnvironment = new TestHostEnvironment { EnvironmentName = environmentName };
        
        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "Reporting:EnableIntegrationProof", optIn ? "true" : "false" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var module = new Module();

        // Act: Invoke the real registration path with trusted host environment
        module.RegisterServices(services, configuration, hostEnvironment);

        // Assert
        var hasProofContext = services.Any(sd => sd.ImplementationType == typeof(ProofReportExecutionContext));
        var hasProofPolicy = services.Any(sd => sd.ImplementationType == typeof(ProofReportAccessPolicy));
        var hasProofScope = services.Any(sd => sd.ImplementationType == typeof(ProofReportDataScopeProvider));
        var hasProofExecutor = services.Any(sd => sd.ImplementationType == typeof(ProofQueryExecutor));

        if (shouldRegister)
        {
            Assert.True(hasProofContext, "ProofReportExecutionContext should be registered in Debug + Development + opt-in=true.");
            Assert.True(hasProofPolicy, "ProofReportAccessPolicy should be registered in Debug + Development + opt-in=true.");
            Assert.True(hasProofScope, "ProofReportDataScopeProvider should be registered in Debug + Development + opt-in=true.");
            Assert.True(hasProofExecutor, "ProofQueryExecutor should be registered in Debug + Development + opt-in=true.");
        }
        else
        {
            Assert.False(hasProofContext, $"Proof services must NOT be registered under {environmentName} with optIn={optIn}.");
            Assert.False(hasProofPolicy, $"Proof access policy must NOT be registered under {environmentName} with optIn={optIn}.");
            Assert.False(hasProofScope, $"Proof scope provider must NOT be registered under {environmentName} with optIn={optIn}.");
            Assert.False(hasProofExecutor, $"Proof query executor must NOT be registered under {environmentName} with optIn={optIn}.");
        }
    }

    [Fact]
    public void DebugModule_RealRegisterServices_FromServicesCollection_RejectsProduction()
    {
        // When hostEnvironment is not passed directly but registered in IServiceCollection (ASP.NET Core pattern)
        var services = new ServiceCollection();
        var prodEnv = new TestHostEnvironment { EnvironmentName = Environments.Production };
        services.AddSingleton<IHostEnvironment>(prodEnv);

        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
        {
            { "Reporting:EnableIntegrationProof", "true" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var module = new Module();

        // Act: invoke 2-parameter overload
        module.RegisterServices(services, configuration);

        // Assert: must fail closed because IHostEnvironment in services is Production
        Assert.False(services.Any(sd => sd.ImplementationType == typeof(ProofReportExecutionContext)),
            "Proof services must NOT be registered when IHostEnvironment in services is Production.");
    }
#endif

    #endregion
}
