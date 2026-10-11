using Shared.Infrastructure.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SettingModule.Entities;
public class SettingDefinition : BaseEntity<Guid>
{
    /// <summary>
    /// Unique name of the setting.
    /// </summary>
    [NotNull]
    public string Name { get; set; }
    [NotNull]
    public string ModuleName { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    /// <summary>
    /// Default value of the setting.
    /// </summary>
    public string  DefaultValue { get; set; }
    /// <summary>
    /// Can clients see this setting and it's value.
    /// It maybe dangerous for some settings to be visible to clients (such as an email server password).
    /// Default: false.
    /// </summary>
    public bool IsVisibleToClients { get; set; }
    /// <summary>
    /// A list of allowed providers to get/set value of this setting.
    /// An empty list indicates that all providers are allowed.
    /// </summary>
    public List<string> Providers { get; }

    /// <summary>
    /// Is this setting inherited from parent scopes.
    /// Default: True.
    /// </summary>
    public bool IsInherited { get; set; }
    /// <summary>
    /// Is this setting stored as encrypted in the data source.
    /// Default: False.
    /// </summary>
    public bool IsEncrypted { get; set; }
    /// <summary>
    /// Is this setting enabled.
    /// </summary>
    public bool IsEnabled { get; set; }
}