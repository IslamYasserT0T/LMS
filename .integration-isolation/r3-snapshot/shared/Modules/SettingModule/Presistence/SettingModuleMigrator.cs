using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SettingModule.Presistence.Context;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Presistence;

public class SettingModuleMigrator : IModuleDbMigrator
{
    public void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SettingDbContext>();
        context.Database.Migrate();
    }

    public bool IsMigrationApplied(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SettingDbContext>();
        var pendingMigrations = context.Database.GetPendingMigrations();
        return !pendingMigrations.Any();
    }
}