using Microsoft.EntityFrameworkCore;
using SettingModule.Contracts;
using SettingModule.Entities;
using SettingModule.Presistence.Context;
using System.Collections.Immutable;
using System.Threading.Tasks;
namespace SettingModule.Services;
public class SettingDefinationContext : ISettingDefinationContext
{
    private  SettingDbContext _context;

    public  SettingDefinationContext(SettingDbContext context)
    {
        _context = context;
    }
    public async Task<Dictionary<string, SettingDefinition>> GetList(CancellationToken cancellationToken = default)
    {
      return await _context.SettingDefinitions
                 .AsNoTracking() 
                 .ToDictionaryAsync(sd => sd.Name);
    }
    public virtual async Task<SettingDefinition?> GetOrNull(string name)
    {
        return await _context.SettingDefinitions.FirstOrDefaultAsync(sd => sd.Name == name);
    }

    public virtual async Task<IReadOnlyList<SettingDefinition>> GetAll()
    {
        return await _context.SettingDefinitions.ToListAsync();
    }

    public virtual async Task Add(params SettingDefinition[] definitions)
    { 
        if (definitions.Length == 0)
            return;
        foreach (var item in definitions)
            if (!_context.SettingDefinitions.Any(x => x.Name == item.Name))
            {
                await _context.SettingDefinitions.AddAsync(item);
                await _context.SaveChangesAsync();
            }

      
    }

    public async Task Update(SettingDefinition definitions)
    {
        _context.SettingDefinitions.Update(definitions);
        await _context.SaveChangesAsync();
    }
}
