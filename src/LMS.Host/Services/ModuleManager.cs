using LMS.Host.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
    private readonly List<ModuleInfo> _modules = new();
    private readonly List<Assembly> _assemblies = new();
    private readonly List<ModuleInfo> _moduleUis = new();
    private readonly List<Assembly> _additionalBlazorAssemblies = new();
    private IServiceCollection? _services;
    private IConfiguration? _configuration;
    private IHostEnvironment? _hostEnvironment;
    private List<NavMenuItem> _menuItems = new();
    private static bool _satelliteResolverRegistered;
    private static readonly object _resolverLock = new();

    ///<inheritdoc/>
    public bool Loaded { get; set; }

    #region Public Methods
    ///<inheritdoc/>
    public Task LoadModules(IServiceCollection services, IConfiguration configuration, IHostEnvironment? hostEnvironment = null)
    {
        _services = services;
        _configuration = configuration;
        _hostEnvironment = hostEnvironment 
            ?? services.FirstOrDefault(d => d.ServiceType == typeof(IHostEnvironment))?.ImplementationInstance as IHostEnvironment;

        EnsureSatelliteAssemblyResolverRegistered();

        LoadModule();
        Loaded = true;
        return Task.CompletedTask;
    }

    ///<inheritdoc/>
    public async Task InitializeRuntimeAsync(IServiceProvider serviceProvider)
    {
        await ExecuteSettingProviders(serviceProvider);
        ExecuteNavProviders(serviceProvider);
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
    ///<inheritdoc/>
    public List<Assembly> GetAdditionalBlazorAssemblies()
    {
        return _additionalBlazorAssemblies;
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

    private void ExecuteNavProviders(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var providers = scope.ServiceProvider.GetServices<INavigationProvider>();
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

                        // Discover companion Blazor/UI assemblies in the same plugin directory.
                        // These are DLLs that are NOT the plugin entry point but carry @page-decorated
                        // Razor components (e.g. IY.Auth.UI.dll, IY.Reporting.Blazor.dll).
                        LoadCompanionBlazorAssemblies(dir);

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

    /// <summary>
    /// Scans the given plugin directory for companion DLLs containing Blazor route components
    /// (*.UI.dll, *.Blazor.dll) and loads them so their @page routes can be registered
    /// with the Blazor Router and server-side endpoint discovery.
    /// Explicitly validates that candidate assemblies contain RouteAttribute-bearing components.
    /// </summary>
    private void LoadCompanionBlazorAssemblies(string pluginDir)
    {
        var companionPatterns = new[] { ".UI.dll", ".Blazor.dll" };

        foreach (var dllPath in Directory.GetFiles(pluginDir, "*.dll"))
        {
            var fileName = Path.GetFileName(dllPath);

            if (!companionPatterns.Any(p => fileName.EndsWith(p, StringComparison.OrdinalIgnoreCase)))
                continue;

            var assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            var existingAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));

            Assembly? candidateAsm = existingAsm;
            if (candidateAsm == null)
            {
                try
                {
                    candidateAsm = Assembly.LoadFrom(dllPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load candidate companion Blazor assembly {dllPath}: {ex.Message}");
                    continue;
                }
            }

            if (candidateAsm != null && IsRouteBearingAssembly(candidateAsm))
            {
                if (!_additionalBlazorAssemblies.Contains(candidateAsm))
                {
                    _additionalBlazorAssemblies.Add(candidateAsm);
                    Console.WriteLine($"Discovered route-bearing companion assembly: {candidateAsm.GetName().Name}");
                }
            }
        }
    }

    private static bool IsRouteBearingAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes().Any(t =>
                typeof(Microsoft.AspNetCore.Components.IComponent).IsAssignableFrom(t) &&
                t.GetCustomAttributes().Any(a => a.GetType().FullName == "Microsoft.AspNetCore.Components.RouteAttribute"));
        }
        catch
        {
            return false;
        }
    }

    private void RegisterModuleServices(Type moduleType, ModuleInfo moduleInfo)
    {
        var module = (IModule)Activator.CreateInstance(moduleType)!;
        
        module.RegisterServices(_services!, _configuration!, _hostEnvironment);
        _modules.Add(moduleInfo);

        switch (true)
        {
            case var _ when typeof(IUiModule).IsAssignableFrom(module.ModuleType):
                _moduleUis.Add(moduleInfo);
                break;
        }
    }

    private static void EnsureSatelliteAssemblyResolverRegistered()
    {
        if (_satelliteResolverRegistered) return;
        lock (_resolverLock)
        {
            if (_satelliteResolverRegistered) return;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveSatelliteAssemblies;
            _satelliteResolverRegistered = true;
        }
    }

    private static Assembly? ResolveSatelliteAssemblies(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        var cultureName = assemblyName.CultureName;
        if (string.IsNullOrEmpty(cultureName))
            return null;

        var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        if (!Directory.Exists(pluginsDir))
            return null;

        foreach (var dir in Directory.GetDirectories(pluginsDir))
        {
            var directCulturePath = Path.Combine(dir, cultureName, $"{assemblyName.Name}.dll");
            if (File.Exists(directCulturePath))
            {
                return Assembly.LoadFrom(directCulturePath);
            }

            var lang = cultureName.Split('-')[0];
            var langPath = Path.Combine(dir, lang, $"{assemblyName.Name}.dll");
            if (File.Exists(langPath))
            {
                return Assembly.LoadFrom(langPath);
            }
        }

        return null;
    }

    #endregion

}