using LMS.Host.Services;

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
    Task LoadModules(IServiceCollection services, IConfiguration configuration);
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
}
