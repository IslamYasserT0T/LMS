using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using LMS.Host.Dtos;
using LMS.Host.Services;
using Xunit;

namespace LMS.ReportingModule.Test;

public class PluginDeploymentRegressionTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _solutionRoot;

    public PluginDeploymentRegressionTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "LMS_PluginRegression_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        // Resolve D:\LMS solution root
        var current = AppContext.BaseDirectory;
        var dir = new DirectoryInfo(current);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "LMS.sln")))
        {
            dir = dir.Parent;
        }
        _solutionRoot = dir?.FullName ?? @"D:\LMS";
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { }
    }

    [Fact]
    public void ReleaseManifestResolution_ResolvesReleasePath_AndContainsNoDebugPath()
    {
        // Arrange
        var manifestsFolder = Path.Combine(_solutionRoot, "ManifestPlugins");
        var outputManifest = Path.Combine(_tempDir, "manifest_release");
        var manager = new ModuleManifestsManager();

        // Act: Process manifests in Release configuration
        manager.ProcessManifests(manifestsFolder, outputManifest, "Release");

        // Assert
        Assert.True(File.Exists(outputManifest), "Manifest output file was not created.");
        var json = File.ReadAllText(outputManifest);
        var root = JsonSerializer.Deserialize<ModulesRoot>(json);
        Assert.NotNull(root);

        var reportingModule = root.Modules
            .SelectMany(m => m.Modules)
            .FirstOrDefault(m => m.Name == "LMS.ReportingModule");

        Assert.NotNull(reportingModule);
        Assert.False(string.IsNullOrWhiteSpace(reportingModule.CopyFolder));

        // Must resolve Release and MUST NOT contain Debug
        Assert.Contains("Release", reportingModule.CopyFolder, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Debug", reportingModule.CopyFolder, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DebugManifestResolution_ResolvesDebugPath()
    {
        // Arrange
        var manifestsFolder = Path.Combine(_solutionRoot, "ManifestPlugins");
        var outputManifest = Path.Combine(_tempDir, "manifest_debug");
        var manager = new ModuleManifestsManager();

        // Act: Process manifests in Debug configuration
        manager.ProcessManifests(manifestsFolder, outputManifest, "Debug");

        // Assert
        Assert.True(File.Exists(outputManifest), "Manifest output file was not created.");
        var json = File.ReadAllText(outputManifest);
        var root = JsonSerializer.Deserialize<ModulesRoot>(json);
        Assert.NotNull(root);

        var reportingModule = root.Modules
            .SelectMany(m => m.Modules)
            .FirstOrDefault(m => m.Name == "LMS.ReportingModule");

        Assert.NotNull(reportingModule);
        Assert.Contains("Debug", reportingModule.CopyFolder, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Release", reportingModule.CopyFolder, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeployedReleaseReportingModule_Metadata_DoesNotContainProofTypes()
    {
        var releaseDll = Path.Combine(_solutionRoot, "modules", "ReportingModule", "LMS.ReportingModule", "bin", "Release", "net9.0", "LMS.ReportingModule.dll");
        Assert.True(File.Exists(releaseDll), $"Release DLL must exist at {releaseDll}. Run Release build first.");

        var assemblyBytes = File.ReadAllBytes(releaseDll);
        var assembly = Assembly.Load(assemblyBytes);

        var typeNames = assembly.GetTypes().Select(t => t.Name).ToList();

        Assert.DoesNotContain("ProofReportExecutionContext", typeNames);
        Assert.DoesNotContain("ProofReportAccessPolicy", typeNames);
        Assert.DoesNotContain("ProofReportDataScopeProvider", typeNames);
        Assert.DoesNotContain("ProofQueryExecutor", typeNames);
        Assert.DoesNotContain("IntegrationProof", typeNames);
    }

    [Fact]
    public void DeployedDependencies_DIAbstractions_ResolvesNet9Compatible_Not10()
    {
        var releaseDepsJson = Path.Combine(_solutionRoot, "modules", "ReportingModule", "LMS.ReportingModule", "bin", "Release", "net9.0", "LMS.ReportingModule.deps.json");
        Assert.True(File.Exists(releaseDepsJson), $"Release deps.json must exist at {releaseDepsJson}");

        var depsContent = File.ReadAllText(releaseDepsJson);

        // Assert no 10.x version of Microsoft.Extensions.DependencyInjection.Abstractions exists
        Assert.DoesNotContain("Microsoft.Extensions.DependencyInjection.Abstractions/10.", depsContent);
        
        // Assert 9.x is resolved
        Assert.Contains("Microsoft.Extensions.DependencyInjection.Abstractions/9.", depsContent);
    }

    [Fact]
    public void DeployedDependencies_IYReporting_IsVersion101_No100Remains()
    {
        var releaseDepsJson = Path.Combine(_solutionRoot, "modules", "ReportingModule", "LMS.ReportingModule", "bin", "Release", "net9.0", "LMS.ReportingModule.deps.json");
        Assert.True(File.Exists(releaseDepsJson), $"Release deps.json must exist at {releaseDepsJson}");

        var depsContent = File.ReadAllText(releaseDepsJson);

        // Must NOT contain 1.0.0 for IY.Reporting
        Assert.DoesNotContain("IY.Reporting/1.0.0", depsContent);
        Assert.DoesNotContain("IY.Reporting.AspNetCore/1.0.0", depsContent);
        Assert.DoesNotContain("IY.Reporting.Blazor/1.0.0", depsContent);

        // Must contain 1.0.1
        Assert.Contains("IY.Reporting/1.0.1", depsContent);
        Assert.Contains("IY.Reporting.AspNetCore/1.0.1", depsContent);
        Assert.Contains("IY.Reporting.Blazor/1.0.1", depsContent);
    }
}
