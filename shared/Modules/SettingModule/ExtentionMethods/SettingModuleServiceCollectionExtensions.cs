using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SettingModule.Contracts;
using SettingModule.Presistence.Context;
using SettingModule.Services;
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
           options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddSingleton<ISettingDefinationManager, SettingDefinationManager>();
        services.AddScoped<ISettingDefinationContext, SettingDefinationContext>();
        return services;
    }
}
