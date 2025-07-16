using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Application;

public class Module : IModule
{
    public string Description => "The user management module api";
    public string Name => "user management Module api";
    public string NavigationLinkName => " ";
    public string URL => " ";

    public Type ModuleType => typeof(IData);

    public string StaticFilesPath => " ";

    public bool IsEnabled => true;


    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        //services.AddControllers().AddApplicationPart(typeof(SampleController).Assembly);

    }
}
