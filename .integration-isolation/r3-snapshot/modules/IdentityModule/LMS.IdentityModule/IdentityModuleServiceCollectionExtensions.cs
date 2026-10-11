using IY.Auth.AspNetCore;
using IY.Auth.AspNetCore.Endpoints;
using IY.Auth.Contracts;
using Microsoft.AspNetCore.Routing;
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
            .UseOpenIddict(creds =>
            {
                configuration.GetSection("IYAuth:Credentials").Bind(creds);
            });

        services.AddScoped<IModuleDbMigrator, IdentityModuleDbMigrator>();

        // Register the IY.Auth endpoint mapping so Host can invoke it without an IY.Auth dependency.
        services.AddSingleton<Action<IEndpointRouteBuilder>>(endpoints => endpoints.MapIYAuthEndpoints());

        return services;
    }
}
