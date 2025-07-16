using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Infrastructure.Presistence.Context;

namespace UserManagementModule.Infrastructure.Presistence;

public class UserManagementModuleDbContextFactory : IDesignTimeDbContextFactory<UserManagmentModuleDBContext>
{

    UserManagmentModuleDBContext IDesignTimeDbContextFactory<UserManagmentModuleDBContext>.CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserManagmentModuleDBContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=LMSLocal;Username=postgres;Password=123;Include Error Detail=true");

        return new UserManagmentModuleDBContext(optionsBuilder.Options);
    }
}
