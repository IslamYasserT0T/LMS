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
        IServiceScope scope;
        ISettingDefinationContext context;
        GetSettingContextService(out scope, out context);
        _settings = await context.GetList();
        return _settings;


    }

    public async Task<Dictionary<string, SettingDefinition>> Update(SettingDefinition settingDefinition)
    {
        IServiceScope scope;
        ISettingDefinationContext context;
        GetSettingContextService(out scope, out context);
        await context.Update(settingDefinition);
        _settings = await context.GetList();
        return _settings;
    }

    #region Private Methods
    private void GetSettingContextService(out IServiceScope scope, out ISettingDefinationContext context)
    {
        scope = _scopeFactory.CreateScope();
        context = scope.ServiceProvider.GetRequiredService<ISettingDefinationContext>();
    }

    #endregion
}
