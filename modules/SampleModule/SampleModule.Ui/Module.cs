using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleModule.Application.Extensions;
using SampleModule.Infrastructure.Extensions;
using Shared.Core.Contracts;
using SampleModule.Ui.Nav;
using Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Ui;

public class Module : IModule
{
    public string Description => "The first module";
    public string Name => "Sample Module Ui";
    public string NavigationLinkName => "Sample Module Ui";
    public string URL => "Component/razor";

    public Type ModuleType => typeof(IUiModule);

    public string StaticFilesPath => "Product.Module.Ui/wwwroot";

    public bool IsEnabled => true;

    
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INavigationProvider, ProductNavigationProvider>();
    }
}
