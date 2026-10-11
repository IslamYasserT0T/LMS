using IY.Auth.Contracts;
using IY.Auth.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;

namespace LMS.IdentityModule;

public sealed class IdentityModuleDbMigrator : IModuleDbMigrator
{
    public void ApplyMigrations(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IdentityDbContext>();
        if (context.Database.IsSqlite())
        {
            context.Database.EnsureCreated();
            return;
        }
        var migrator = serviceProvider.GetRequiredService<IIdentityDatabaseMigrator>();
        migrator.MigrateAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public bool IsMigrationApplied(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<IdentityDbContext>();
        if (context.Database.IsSqlite()) return false;
        var migrator = serviceProvider.GetRequiredService<IIdentityDatabaseMigrator>();
        var hasPending = migrator.HasPendingMigrationsAsync(CancellationToken.None).GetAwaiter().GetResult();
        return !hasPending;
    }
}
