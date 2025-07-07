using SettingModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Contracts;
public interface ISettingDefinitionContext
{
    SettingDefinition? GetOrNull(string name);

    IReadOnlyList<SettingDefinition> GetAll();

    void Add(params SettingDefinition[] definitions );
}
