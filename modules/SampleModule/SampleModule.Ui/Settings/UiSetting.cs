using SettingModule.Contracts;
using SettingModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Ui.Settings;

public class UiSetting : ISettingDefinationProvider
{
    public string ModuleName = "SettingModules ";
    public async Task Define(ISettingDefinationContext context)
    {
        await context.Add(new SettingDefinition { Name = "Host", ModuleName = ModuleName, DefaultValue = "localhost", IsEncrypted = false });
    }

}
