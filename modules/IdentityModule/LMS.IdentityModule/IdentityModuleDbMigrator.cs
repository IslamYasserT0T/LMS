using IY.Auth.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;

namespace LMS.IdentityModule;

public sealed class IdentityModuleDbMigrator : IModuleDbMigrator
{
    public void ApplyMigrations(IServiceProvider serviceProvider)
    {
        var migrator = serviceProvider.GetRequiredService<IIdentityDatabaseMigrator>();
        migrator.MigrateAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public bool IsMigrationApplied(IServiceProvider serviceProvider)
    {
        var migrator = serviceProvider.GetRequiredService<IIdentityDatabaseMigrator>();
        var hasPending = migrator.HasPendingMigrationsAsync(CancellationToken.None).GetAwaiter().GetResult();
        return !hasPending;
    }
}