using LMS.Host.Services;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace LMS.Host.Components;

public class ModuleRouter : Router, IComponent, IDisposable
{
    private List<Assembly> _additionalAssemblies = new();

    public new void Attach(RenderHandle renderHandle)
    {
        base.Attach(renderHandle);
        ModuleManager.OnModulesChanged += OnModulesChanged;

        // Load initial modules
        var initialModules = ModuleManager.GetUiModules();
        if (initialModules.Any())
        {
            _additionalAssemblies.AddRange(initialModules.Select(m => m.Assembly));
            UpdateRouter();
        }
    }

    public new void Dispose()
    {
        base.Dispose();
        ModuleManager.OnModulesChanged -= OnModulesChanged;
    }

    private async void OnModulesChanged(IEnumerable<ModuleInfo> modules)
    {
        _additionalAssemblies = modules.Select(m => m.Assembly).ToList();
        await UpdateRouter();
    }

    private async Task UpdateRouter()
    {
        var dict = new Dictionary<string, object>
        {
            { "AdditionalAssemblies", _additionalAssemblies }
        };

        var pv = ParameterView.FromDictionary(dict);
        await SetParametersAsync(pv);
    }
}

