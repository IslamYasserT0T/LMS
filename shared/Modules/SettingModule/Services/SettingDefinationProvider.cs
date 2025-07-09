using SettingModule.Contracts;
using SettingModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Services;
public  class SettingDefinationProvider  : ISettingDefinationProvider
{
    public string ModuleName = "SettingModules ";

    public  void Define(ISettingDefinationContext context)
    {
        context.Add(new SettingDefinition { Name = "Host", ModuleName = ModuleName, DefaultValue = "localhost", IsEncrypted = false });
    }
}