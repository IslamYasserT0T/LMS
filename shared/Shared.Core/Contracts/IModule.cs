using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core.Contracts;
public interface IModule
{
    string Name { get; }
    string Description { get; }
    Type ModuleType { get; }
    bool IsEnabled { get; }
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
    void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment? hostEnvironment)
    {
        RegisterServices(services, configuration);
    }
}
