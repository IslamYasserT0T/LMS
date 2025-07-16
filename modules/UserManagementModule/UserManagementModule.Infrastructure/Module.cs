using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Contracts;
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

namespace SampleModule.Infrastructure;

public class Module : IModule
{
    public string Description => "The user management module infrastructure";
    public string Name => "User management Module infrastructure";
    public string NavigationLinkName => " ";
    public string URL => " ";

    public Type ModuleType => typeof(IData);

    public string StaticFilesPath => " ";

    public bool IsEnabled => true;


    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
       .AddEntityFrameworkStores<UserManagmentModuleDBContext>();

        services.AddDbContext<UserManagmentModuleDBContext>(options =>
          options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IModuleDbMigrator, UserManagementModuleMigrator>();
        services.AddScoped<IBaseDbContext, UserManagmentModuleDBContext>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
