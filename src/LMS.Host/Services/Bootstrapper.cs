using AuthorizationModule.ExtentionMethods;
using LMS.Host.Components;
using LMS.Host.Contracts;
using LMS.Host.ExtensionsMethods;
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

    public Bootstrapper(WebApplicationBuilder builder)
    {
        _builder = builder;
    }
    public async Task PreInitialize()
    {
        var filesCopier = new ModuleFolderCopier();
        var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "manifest.json");
        var destinationRoot = Path.Combine(AppContext.BaseDirectory,"plugins");

        await ModuleFolderCopier.CopyFoldersFromManifestAsync(manifestPath, destinationRoot);

        _builder.Services.AddDbContext<BaseDbContext>(options =>
          options.UseNpgsql(_builder.Configuration.GetConnectionString("Default")));

        _builder.Services.AddSettingModule(_builder.Configuration);
        _builder.Services.AddAuthorizationModule(_builder.Configuration);
        _builder.Services.AddControllers();

        _builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        _builder.Services.AddServerSideBlazor();
        _builder.Services.AddMudServices();
        _builder.Services.AddSingleton<IModuleManager, ModuleManager>();

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
        await Task.Yield();

    }

    public async Task Initialize(IModuleManager moduleManager)
    {

         await moduleManager.LoadModules(_builder.Services, _builder.Configuration);

    }

    public async Task PostInitialize(IModuleManager moduleManager)
    {
        var mvcBuilder = _builder.Services.AddRazorPages();
        var modules = moduleManager.GetUiModules();
        foreach (var assembly in modules)
        {
            mvcBuilder.AddApplicationPart(assembly.Assembly);
        }
        _builder.Services.AddModuleAutoMappers(modules.Select(m => m.Assembly).ToArray());


        await Task.Yield();
    }

    public void ConfigureApplication(WebApplication app, IModuleManager moduleManager)
    {
        ApplyMigrations(app);

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
                .AddAdditionalAssemblies(moduleManager.GetModules().Select(m => m.Assembly).ToArray());
    }

    private static void ApplyMigrations(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var migrators = scope.ServiceProvider.GetServices<IModuleDbMigrator>();
            foreach (var migrator in migrators)
            {
                migrator.ApplyMigrations(scope.ServiceProvider);
            }
        }
    }
}
