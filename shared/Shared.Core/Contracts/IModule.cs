using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    string NavigationLinkName { get; }
    string URL { get; }
    Type ModuleType { get; }
    string StaticFilesPath { get; }
    bool IsEnabled { get; }
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
