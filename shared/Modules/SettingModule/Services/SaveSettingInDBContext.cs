using SettingModule.Contracts;
using SettingModule.Entities;
using SettingModule.Presistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Services;
public class SaveSettingInDBContext(SettingDbContext context, ISettingDefinationContext settingDefinationContext)
{
    private readonly SettingDbContext _context = context;
    private readonly ISettingDefinationContext _settingDefinationContext;
    public  List <SettingDefinition> Settings { get; set; }
    public async Task<bool> SaveSettings()
    {
        Settings = _settingDefinationContext.GetAll().ToList();
        foreach (var item in Settings)
        {
            if (_context.SettingDefinitions.Any(x => x.Name == item.Name))
            {
                _context.SettingDefinitions.Update(item);
            }
            else
            {
                _context.SettingDefinitions.Add(item);
            }
        }
        await _context.SaveChangesAsync();
        return true;
    }
}
