using Microsoft.Extensions.DependencyInjection;
using SettingModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Contracts;

public interface ISettingDefinationManager
{
    Task<Dictionary<string, SettingDefinition>> InitializeAsync();
}
