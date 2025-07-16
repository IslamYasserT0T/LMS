using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementModule.Infrastructure.Presistence.Context;

public class UserManagmentModuleDBContext : BaseDbContext
{
    public UserManagmentModuleDBContext(DbContextOptions<UserManagmentModuleDBContext> options) : base(options)
    {
    }

    #region DbSets

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserManagmentModuleDBContext).Assembly);
    }
}
