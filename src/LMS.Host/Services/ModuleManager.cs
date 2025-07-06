using ModulePoc.Shared;
using Shared.Core;
using System.Reflection;

namespace LMS.Host.Services;

public static class ModuleManager
{
    private static readonly List<ModuleInfo> _modules = new();
    private static readonly List<ModuleInfo> _moduleApis = new();
    private static readonly List<ModuleInfo> _moduleUis = new();
    private static IServiceCollection _services;

    public static event Action<IEnumerable<ModuleInfo>> OnModulesChanged;

    public static bool Loaded { get; private set; }

    public static void LoadModules(IServiceCollection services)
    {
        _services = services;
        LoadModule();

        OnModulesChanged?.Invoke(_modules);
        Loaded = true;
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
                Console.WriteLine($"Loaded module from {pluginDll}");

            }

        }

    }
    private static void RegisterModuleServices(Type moduleType, ModuleInfo moduleInfo)
    {
        var module = (IModule)Activator.CreateInstance(moduleType)!;
        module.RegisterServices(_services);
        switch (true)
        {
            case var _ when typeof(IApiModule).IsAssignableFrom(module.ModuleType):
                _moduleApis.Add(moduleInfo);
                break;

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

    public static List<ModuleInfo> GetApiModules()
    {
        return _moduleApis;
    }

    public static List<ModuleInfo> GetModules()
    {
        return _modules;
    }
}
