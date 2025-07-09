using Microsoft.EntityFrameworkCore;
using ModulePoc.Shared;
using SettingModule.Contracts;
using SettingModule.Presistence.Context;
using SettingModule.Services;
using Shared.Core;
using System.Reflection;
using System.Threading.Tasks;

namespace LMS.Host.Services;

public static class ModuleManager
{
    private static readonly List<ModuleInfo> _modules = new();
    private static readonly List<ModuleInfo> _moduleUis = new();
    private static IServiceCollection _services;

    public static event Action<IEnumerable<ModuleInfo>> OnModulesChanged;

    public static bool Loaded { get; private set; }

    public static async Task LoadModules(IServiceCollection services)
    {
        _services = services;
        LoadModule();
        using var provider = services.BuildServiceProvider();
        await ExecuteSettingProviders(provider);
        OnModulesChanged?.Invoke(_modules);
        Loaded = true;
    }
    private static async Task ExecuteSettingProviders(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SettingDbContext>();

        var context = new SettingDefinationContext(dbContext);

        var assemblies = _modules.Select(m => m.Assembly).Distinct();

        foreach (var assembly in assemblies)
        {
            var providerTypes = assembly.GetTypes()
                .Where(t => typeof(ISettingDefinationProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in providerTypes)
            {
                var instance = (ISettingDefinationProvider?)scope.ServiceProvider.GetService(type)
                              ?? (ISettingDefinationProvider?)Activator.CreateInstance(type);

                if (instance != null)
                {
                   await instance.Define(context);
                }
            }
        }
    }
    private static void LoadModule()
    {
        var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");

        if (Path.Exists(pluginsDir))
        {
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var dirName = Path.GetFileName(dir);
                var pluginDll = Path.Combine(dir, dirName + ".dll");
                if (File.Exists(pluginDll))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(pluginDll);
                        var moduleTypes = assembly.GetTypes()
                                        .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                                        .ToList();
 
                        foreach (var moduleType in moduleTypes)
                        {
                            if (ModuleInfo.TryCreateModule(assembly, out var moduleInfo))
                            {
                                RegisterModuleServices(moduleType, moduleInfo);
                            }
                        }

                        Console.WriteLine($"Loaded module from {pluginDll}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load module from {pluginDll}: {ex.Message}");
                    }
                }
            }

        }

    }

    private static void RegisterModuleServices(Type moduleType, ModuleInfo moduleInfo)
    {
        var module = (IModule)Activator.CreateInstance(moduleType)!;
        module.RegisterServices(_services);
        _modules.Add(moduleInfo);

        switch (true)
        {
           

            case var _ when typeof(IUiModule).IsAssignableFrom(module.ModuleType):
                _moduleUis.Add(moduleInfo);
                break;

            case var _ when typeof(IDataModule).IsAssignableFrom(module.ModuleType):
                _modules.Add(moduleInfo);
                break;

            default:
                _modules.Add(moduleInfo);
                break;
        }
    }

    public static List<ModuleInfo> GetUiModules()
    {
        return _moduleUis;
    }

    public static List<ModuleInfo> GetModules()
    {
        return _modules;
    }
}
