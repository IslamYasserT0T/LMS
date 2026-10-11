using AuthorizationModule.Presistence.Context;
using Casbin;
using Casbin.Model;
using Casbin.Persist;
using Casbin.Persist.Adapter.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SettingModule.Presistence;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationModule.ExtentionMethods;

public static class AuthorizationModuleServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationModule(this IServiceCollection services , IConfiguration configuration)
    {
        services.AddDbContext<AuthorizationDbContext>(options =>
          options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IAdapter>(provider =>
        {
            var dbContext = provider.GetRequiredService<AuthorizationDbContext>();
            return new EFCoreAdapter<int>(dbContext);
        });
        services.AddScoped<IEnforcer>(sp =>
        {

            var modelPath = Path.Combine("wwwroot", "plugins", "AuthorizationModule", "casbin_model.conf");

            var model = DefaultModel.CreateFromFile(modelPath);

            var adapter = sp.GetRequiredService<IAdapter>();
            return new Enforcer(model, adapter);
        });
        services.AddScoped<IModuleDbMigrator, AuthorizeModuleMigrator>();

        return services;
    }
}
