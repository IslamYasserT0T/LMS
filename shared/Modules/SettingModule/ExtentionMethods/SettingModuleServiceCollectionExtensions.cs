using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SettingModule.Contracts;
using SettingModule.Presistence;
using SettingModule.Presistence.Context;
using SettingModule.Services;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.ExtentionMethods;

public static class SettingModuleServiceCollectionExtensions
{
    public static IServiceCollection AddSettingModule(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<SettingDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default");
            if (string.Equals(configuration["IntegrationTest:DatabaseProvider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(connectionString);
            else
                options.UseNpgsql(connectionString);
        });
        services.AddSingleton<ISettingDefinationManager, SettingDefinationManager>();
        services.AddScoped<ISettingDefinationContext, SettingDefinationContext>();
        services.AddScoped<IModuleDbMigrator, SettingModuleMigrator>();

        return services;
    }
}
