using SettingModule.Contracts;
using SettingModule.Entities;
using System.Collections.Immutable;
namespace SettingModule.Services;
public class SettingDefinitionContext : ISettingDefinitionContext
{
    protected Dictionary<string, SettingDefinition> Settings { get; }

    public SettingDefinitionContext(Dictionary<string, SettingDefinition> settings)
    {
        Settings = settings;
    }

    public virtual SettingDefinition? GetOrNull(string name)
    {
        return Settings.GetValueOrDefault(name);
    }

    public virtual IReadOnlyList<SettingDefinition> GetAll()
    {
        return Settings.Values.ToImmutableList();
    }

    public virtual void Add(params SettingDefinition[] definitions)
    {
        if (definitions.Length == 0)
        {
            return;
        }

        // Call Service To  Add In Context 
        foreach (var definition in definitions)
        {
            Settings[definition.Name] = definition;
        }
    }
}
