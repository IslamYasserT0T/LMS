using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Core.Contracts;
using System;
using System.Linq;
using IY.Reporting.AspNetCore;
using IY.Reporting.Blazor;
using IY.Reporting.SqlKata;
using Npgsql;
using Microsoft.Data.Sqlite;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace LMS.ReportingModule;

public class Module : IModule
{
    public string Description => "Reporting Module adapter for LMS";
    public string Name => "Reporting Module";
    public string NavigationLinkName => "Reports";
    public string URL => "/reports";

    public Type ModuleType => typeof(Module);

    public string StaticFilesPath => " ";

    public bool IsEnabled => true;

    public const string ProofConfigurationKey = "Reporting:EnableIntegrationProof";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var hostEnv = services.FirstOrDefault(d => d.ServiceType == typeof(IHostEnvironment))?.ImplementationInstance as IHostEnvironment;
        RegisterServices(services, configuration, hostEnv);
    }

    public void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment? hostEnvironment)
    {
        // Minimal registration for Gate L1/L2 to prove module loading and package consumption.
        // Full configuration (context, policies, DbContext) belongs to later tasks.
        
#if DEBUG
        var isProduction = hostEnvironment?.IsProduction() ?? false;
        var isProofEnabled = configuration.GetValue<bool>(ProofConfigurationKey);

        if (!isProduction && isProofEnabled)
        {
            services.AddScoped<IY.Reporting.Models.IReportExecutionContext, ProofReportExecutionContext>();
            services.AddScoped<IY.Reporting.Services.IReportDataScopeProvider, ProofReportDataScopeProvider>();
            services.AddScoped<IY.Reporting.Services.IReportAccessPolicy, ProofReportAccessPolicy>();
            services.AddScoped<IY.Reporting.Provider.QueryExecutorBase, ProofQueryExecutor>();
            services.AddSingleton<IY.Reporting.Schema.ReportSchema>(new IY.Reporting.Schema.ReportSchema("ProofSchema", Array.Empty<IY.Reporting.Schema.ReportTable>(), Array.Empty<IY.Reporting.Schema.ReportOperator>()));
            Console.WriteLine("[LMS.ReportingModule] Integration proof explicitly enabled in non-production environment. Proof harness services registered.");
        }
        else
        {
            if (isProofEnabled && isProduction)
            {
                Console.WriteLine("[LMS.ReportingModule] WARNING: Reporting:EnableIntegrationProof is true, but host environment is Production. Proof harness registration rejected (fail-closed).");
            }
            else
            {
                Console.WriteLine("[LMS.ReportingModule] Debug build active but proof harness disabled or not requested. Proof harness services NOT registered.");
            }
        }
#endif
        
        services.AddLocalization();
        services.AddReporting();
        services.AddSqlKataReporting(serviceProvider =>
        {
            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Reporting requires ConnectionStrings:Default.");

            if (string.Equals(configuration["IntegrationTest:DatabaseProvider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                return CreateSqliteQueryFactory(connectionString);
            }

            return CreatePostgresQueryFactory(connectionString);
        });
        services.AddScoped<IY.Reporting.Models.IReportExecutionContext, LmsReportExecutionContext>();
        services.AddScoped<IY.Reporting.Services.IReportAccessPolicy, LmsReportAccessPolicy>();
        services.AddScoped<IY.Reporting.Services.IReportDataScopeProvider, LmsReportDataScopeProvider>();
        services.AddScoped<IY.Reporting.Export.IReportExportPolicy, LmsReportExportPolicy>();
        services.AddSingleton(LmsReportingSchema.Create());
        services.AddReportingBlazor();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static QueryFactory CreateSqliteQueryFactory(string connectionString)
    {
        return new QueryFactory(new SqliteConnection(connectionString), new SqliteCompiler());
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static QueryFactory CreatePostgresQueryFactory(string connectionString)
    {
        var connectionSettings = new NpgsqlConnectionStringBuilder(connectionString) { CommandTimeout = 30 };
        return new QueryFactory(new NpgsqlConnection(connectionSettings.ConnectionString), new PostgresCompiler());
    }
}
