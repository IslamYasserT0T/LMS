using SettingModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Contracts;
public interface ISettingDefinationContext
{
    Task<SettingDefinition?> GetOrNull(string name);
    Task<Dictionary<string, SettingDefinition>> GetList(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SettingDefinition>> GetAll();

    Task Add(List<SettingDefinition> definitions );
    Task Update(SettingDefinition definitions);
}
