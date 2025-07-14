using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Core;

public class Module : IModule
{
    public string Description => "The sample module core";
    public string Name => "Sample Module core";
    public string NavigationLinkName => " ";
    public string URL => " ";

    public Type ModuleType => typeof(IData);

    public string StaticFilesPath => " ";

    public bool IsEnabled => true;


    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}
