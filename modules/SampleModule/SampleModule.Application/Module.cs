using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleModule.Application.Contracts;
using SampleModule.Application.Extensions;
using SampleModule.Application.Services;
using SampleModule.Infrastructure.Extensions;
using Shared.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Application;

public class Module : IModule
{
    public string Description => "The sample module application";
    public string Name => "Sample Module application";
    public string NavigationLinkName => " ";
    public string URL => " ";

    public Type ModuleType => typeof(IData);

    public string StaticFilesPath => " ";

    public bool IsEnabled => true;


    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISampleAppService, SampleAppService>();
    }
}
