using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Domain.Entities;
using UserManagementModule.Domain.Repositories.Users;
using UserManagementModule.Infrastructure.Presistence;
using UserManagementModule.Infrastructure.Presistence.Context;
using UserManagementModule.Infrastructure.Repositories.Users;

namespace UserManagementModule.Infrastructure.Extensions;

public static class UserManagementServiceCollection
{
    public static IServiceCollection AddUserManagementModuleServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserManagmentModuleDBContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>() 
            .AddEntityFrameworkStores<UserManagmentModuleDBContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IModuleDbMigrator, UserManagementModuleMigrator>();
        services.AddScoped<IBaseDbContext, UserManagmentModuleDBContext>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
