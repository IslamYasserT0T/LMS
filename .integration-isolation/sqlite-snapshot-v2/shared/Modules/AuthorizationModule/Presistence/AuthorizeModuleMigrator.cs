using AuthorizationModule.Presistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Presistence;

public class AuthorizeModuleMigrator : IModuleDbMigrator
{
    public void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthorizationDbContext>();
        if (context.Database.IsSqlite())
        {
            context.Database.EnsureCreated();
            return;
        }
        context.Database.Migrate();
    }

    public bool IsMigrationApplied(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthorizationDbContext>();
        if (context.Database.IsSqlite()) return false;
        var pendingMigrations = context.Database.GetPendingMigrations();
        return !pendingMigrations.Any();
    }
}
