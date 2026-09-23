using IY.Auth.AspNetCore;
using IY.Auth.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;

namespace LMS.IdentityModule;

public static class IdentityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddLMSIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIYAuth()
            .UseEntityFrameworkCore(options =>
            {
                var connectionString = configuration.GetConnectionString("Default");
                var schema = configuration.GetValue<string>("IYAuth:DefaultSchema") ?? "identity";

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema);
                });
            })
            .UseOpenIddict();

        services.AddScoped<IModuleDbMigrator, IdentityModuleDbMigrator>();

        return services;
    }
}