using LMS.Host.Services;
using Shared.Core;
using System.Reflection;

namespace LMS.Host.Contracts;

/// <summary>
/// Module Manager 
/// </summary>
public interface IModuleManager
{
    /// <summary>
    ///  Is Loaded
    /// </summary>
    /// <returns> return true if loaded otherwise false</returns>
     bool Loaded { get; set; }

    /// <summary>
    /// Load Modules
    /// </summary>
    /// <param name="services"> Object of IServiceCollection </param>
    /// <param name="configuration"> Object of IConfiguration </param>
    /// <param name="hostEnvironment"> Optional Object of IHostEnvironment </param>
    Task LoadModules(IServiceCollection services, IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment? hostEnvironment = null);

    /// <summary>
    /// Initialize runtime providers using the final application service provider.
    /// </summary>
    /// <param name="serviceProvider"> Object of IServiceProvider </param>
    Task InitializeRuntimeAsync(IServiceProvider serviceProvider);

    /// <summary>
    /// Get Ui Modules
    /// </summary>
    /// <returns> List of ui ModuleInfo </returns>
    List<ModuleInfo> GetUiModules();
    /// <summary>
    /// Get all Modules
    /// </summary>
    /// <returns> List of all ModuleInfo </returns>
    List<ModuleInfo> GetModules();
    /// <summary>
    /// Get Menu Items
    /// </summary>
    /// <returns> List of NavMenuItem</returns>
    List<NavMenuItem> GetMenuItems();
    /// <summary>
    /// Get All Assemblies
    /// </summary>
    /// <returns> List of Assembly</returns>
    List<Assembly> GetAllAssemblies();
    /// <summary>
    /// Get additional Blazor UI assemblies deployed into plugin folders that contain
    /// route-bearing Razor components (e.g. IY.Auth.UI.dll).  These are separate from
    /// the plugin entry-point assemblies returned by <see cref="GetModules"/>.
    /// </summary>
    /// <returns>List of additional Blazor assemblies</returns>
    List<Assembly> GetAdditionalBlazorAssemblies();
}