using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Infrastructure.Presistence.Context;

namespace UserManagementModule.Infrastructure.Presistence;

public class UserManagementModuleMigrator : IModuleDbMigrator
{
    public void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserManagmentModuleDBContext>();
        context.Database.Migrate();
    }

    public bool IsMigrationApplied(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserManagmentModuleDBContext>();
        var pendingMigrations = context.Database.GetPendingMigrations();
        return !pendingMigrations.Any();
       
    }
}