using LMS.Host.Components;
using LMS.Host.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModulePoc.Host.Pages;
using MudBlazor.Services;
using SettingModule.Contracts;
using SettingModule.ExtentionMethods;
using SettingModule.Presistence.Context;
using SettingModule.Services;
using Shared.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace LMS.Host.Services;

public class Bootstrapper
{
    private readonly WebApplicationBuilder _builder;
    private readonly IModuleManager _moduleManager ;

    public Bootstrapper(WebApplicationBuilder builder)
    {
        _builder = builder;
    }
    public async Task PreInitialize()
    {
        _builder.Services.AddDbContext<BaseDbContext>(options =>
          options.UseNpgsql(_builder.Configuration.GetConnectionString("Default")));

        _builder.Services.AddSettingModule(_builder.Configuration);

        _builder.Services.AddControllers();

        _builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        _builder.Services.AddServerSideBlazor();
        _builder.Services.AddMudServices();

        // Configure Swagger
        _builder.Services.AddEndpointsApiExplorer();
        _builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "ModulePoc API",
                Version = "v1",
                Description = "API for ModulePoc application"
            });
        });
    }    
    
    public async Task Initialize()
    {
        _builder.Services.AddSingleton<IModuleManager, ModuleManager>(); 
        
        var moduleManager = _builder.Services.BuildServiceProvider().GetRequiredService<IModuleManager>();
         await moduleManager.LoadModules(_builder.Services, _builder.Configuration);

    }

    public async Task PostInitialize()
    {
        var mvcBuilder = _builder.Services.AddRazorPages();

        foreach (var assembly in _moduleManager.GetUiModules())
        {
            mvcBuilder.AddApplicationPart(assembly.Assembly);
        }
    }

    public void ConfigureApplication(WebApplication app)
    {
        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ModulePoc API V1");
            });
        }
        app.UseRouting();
        app.UseAntiforgery();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.MapStaticAssets();
        app.UseAuthorization();
        app.MapControllers();
        app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(_moduleManager.GetModules().Select(m => m.Assembly).ToArray());
    }
   
}
