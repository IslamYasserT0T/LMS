using Microsoft.Extensions.DependencyInjection;
using SettingModule.Contracts;
using SettingModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Services;

public class SettingDefinationManager : ISettingDefinationManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private Dictionary<string, SettingDefinition> _settings { get; set; }
    public SettingDefinationManager(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<Dictionary<string, SettingDefinition>> InitializeAsync()
    {
        if (_settings != null) return _settings;
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ISettingDefinationContext>();
        _settings = await context.GetList();
        return _settings;
    }

}
