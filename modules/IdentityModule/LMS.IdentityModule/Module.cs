using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Contracts;

namespace LMS.IdentityModule;

public class Module : IModule
{
    public string Name => "LMS.IdentityModule";
    public string Description => "LMS Identity Module integrating IY.Auth";
    public Type ModuleType => typeof(IModule);
    public bool IsEnabled => true;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLMSIdentityModule(configuration);
    }
}