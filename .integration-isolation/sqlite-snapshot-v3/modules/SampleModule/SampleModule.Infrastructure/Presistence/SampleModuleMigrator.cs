using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleModule.Infrastructure.Presistence.Context;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Infrastructure.Presistence;

public class SampleModuleMigrator : IModuleDbMigrator
{
    public void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleModuleDBContext>();
        if (context.Database.IsSqlite())
        {
            SqliteTestDatabaseInitializer.EnsureInitialized(context, "Samples");
            return;
        }
        context.Database.Migrate();
    }

    public bool IsMigrationApplied(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleModuleDBContext>();
        if (context.Database.IsSqlite()) return SqliteTestDatabaseInitializer.IsInitialized(context, "Samples");
        var pendingMigrations = context.Database.GetPendingMigrations();
        return !pendingMigrations.Any();
    }
}
