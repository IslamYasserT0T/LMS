using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Contracts;
using Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Ui.Nav;

namespace SampleModule.Ui;

public class Module : IModule
{
    public string Description => "The user management module ui";
    public string Name => "User Management Module Ui";
    public string NavigationLinkName => "User management Module Ui";
    public string URL => "Component/razor";

    public Type ModuleType => typeof(IUiModule);

    public string StaticFilesPath => "Product.Module.Ui/wwwroot";

    public bool IsEnabled => true;

    
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INavigationProvider, UserManagementModuleNavigationProvider>();
    }
}
