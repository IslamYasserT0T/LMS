using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Domain.Entities;

namespace UserManagementModule.Infrastructure.Presistence.Context;

public class UserManagmentModuleDBContext : IdentityDbContext<ApplicationUser>
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
