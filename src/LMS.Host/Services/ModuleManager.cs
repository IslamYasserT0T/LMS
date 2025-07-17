using LMS.Host.Contracts;
using Microsoft.EntityFrameworkCore;
using SettingModule.Contracts;
using SettingModule.Presistence.Context;
using SettingModule.Services;
using Shared.Core;
using Shared.Core.Contracts;
using System.Reflection;
using System.Threading.Tasks;

namespace LMS.Host.Services;
///<inheritdoc/>
public class ModuleManager : IModuleManager
{
    private static  List<ModuleInfo> _modules { get; set; } = new();
    private static  List<Assembly> _assemblies { get; set; } = new();
    private  List<ModuleInfo> _moduleUis { get; set; } = new();
    private IServiceCollection _services { get; set; }
    private IConfiguration _configuration { get; set; }
    private static List<NavMenuItem> _menuItems { get; set; } = new();
    ///<inheritdoc/>
    public bool Loaded { get; set; }

    #region Public Methods
    ///<inheritdoc/>
    public async Task LoadModules(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
        LoadModule();
        using var provider = services.BuildServiceProvider();
        await ExecuteSettingProviders(provider);
        ExecuteNavProviders(provider);
        Loaded = true;
    }


    ///<inheritdoc/>
    public List<ModuleInfo> GetUiModules()
    {
        return _moduleUis;
    }
    ///<inheritdoc/>
    public List<NavMenuItem> GetMenuItems()
    {
        return _menuItems;
    }
    ///<inheritdoc/>
    public List<ModuleInfo> GetModules()
    {
        return _modules;
    }
    ///<inheritdoc/>
    public List<Assembly> GetAllAssemblies()
    {
        return _assemblies;
    }
    #endregion

    #region Private Methods
    private async Task ExecuteSettingProviders(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SettingDbContext>();

        var assemblies = _modules.Select(m => m.Assembly).Distinct();

        foreach (var assembly in assemblies)
        {
            var providerTypes = assembly.GetTypes()
                .Where(t => typeof(ISettingDefinationProvider).IsAssignableFrom(t)
                            && !t.IsInterface
                            && !t.IsAbstract);

            foreach (var type in providerTypes)
            {
                var instance = scope.ServiceProvider.GetService(type) as ISettingDefinationProvider
                               ?? ActivatorUtilities.CreateInstance(scope.ServiceProvider, type) as ISettingDefinationProvider;

                if (instance != null)
                {
                    var context = new SettingDefinationContext(dbContext);

                    try
                    {
                        await instance.Define(context);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to define settings in {type.Name}: {ex.Message}");
                    }
                }
            }
        }
    }

    private static void ExecuteNavProviders(ServiceProvider provider)
    {
        var providers = provider.GetServices<INavigationProvider>();
        _menuItems = providers.SelectMany(p => p.GetMenuItems()).ToList();
    }
    private void LoadModule()
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

        _assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());


    }

    private void RegisterModuleServices(Type moduleType, ModuleInfo moduleInfo)
    {
        var module = (IModule)Activator.CreateInstance(moduleType)!;
        
        module.RegisterServices(_services,_configuration);
        _modules.Add(moduleInfo);

        switch (true)
        {
            case var _ when typeof(IUiModule).IsAssignableFrom(module.ModuleType):
                _moduleUis.Add(moduleInfo);
                break;
        }
    }

    #endregion

}
