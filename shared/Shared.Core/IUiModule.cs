using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core;
public interface IUiModule
{
    string Name { get; }
    string Description { get; }
    string NavigationLinkName { get; }
    string URL { get; }
    Type ModuleType { get; }
    string StaticFilesPath { get; }
    bool IsEnabled { get; }
    void RegisterServices(IServiceCollection services);
}
