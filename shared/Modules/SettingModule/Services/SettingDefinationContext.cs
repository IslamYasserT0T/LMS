using Microsoft.EntityFrameworkCore;
using SettingModule.Contracts;
using SettingModule.Entities;
using SettingModule.Presistence.Context;
using System.Collections.Immutable;
namespace SettingModule.Services;
public class SettingDefinationContext : ISettingDefinationContext
{
    public Dictionary<string, SettingDefinition> Settings { get; set; }
    private  SettingDbContext _context;

    public  SettingDefinationContext(SettingDbContext context)
    {

        Settings = new Dictionary<string, SettingDefinition>();
        _context = context;
        Task.Run(() => this.GetList()).Wait();
    }
    public async Task GetList()
    {
        Settings = await _context.SettingDefinitions
                 .AsNoTracking() 
                 .ToDictionaryAsync(sd => sd.Name);
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
        foreach (var definition in definitions)
        {
            Settings[definition.Name] = definition;
        }
    }
}
