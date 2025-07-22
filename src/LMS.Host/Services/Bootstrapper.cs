using AuthorizationModule.ExtentionMethods;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise.RichTextEdit;
using LMS.Host.Components;
using LMS.Host.Contracts;
using LMS.Host.Dtos;
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
using System.Text.Json;
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
              var moduleManifestManager = new ModuleManifestsManager();
        moduleManifestManager.ProcessManifests(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, "ManifestPlugins"),Path.Combine(Directory.GetCurrentDirectory(),"manifest"));

        var manifestPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "manifest"));
        var destinationRoot = Path.Combine(AppContext.BaseDirectory,"plugins");
         if(!Directory.Exists(destinationRoot)) 
            Directory.CreateDirectory(destinationRoot);
        await ModuleFolderCopier.CopyFoldersFromManifestAsync(manifestPath, destinationRoot);

        _builder.Services.AddDbContext<BaseDbContext>(options =>
          options.UseNpgsql(_builder.Configuration.GetConnectionString("Default")));

        _builder.Services.AddSettingModule(_builder.Configuration);
        _builder.Services.AddAuthorizationModule(_builder.Configuration);
        _builder.Services.AddControllers();
        _builder.Services
            .AddBlazorise(options =>
            {
                options.Immediate = true;
            })
            .AddBlazoriseRichTextEdit()
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();
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
        var modulesUi = moduleManager.GetUiModules();
        foreach (var assembly in modulesUi)
        {
            mvcBuilder.AddApplicationPart(assembly.Assembly);
        }
        _builder.Services.AddModuleAutoMappers(moduleManager.GetAllAssemblies());
        _builder.Services.AddModuleValidators(moduleManager.GetAllAssemblies());


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
                if (!migrator.IsMigrationApplied(scope.ServiceProvider))
                {
                    migrator.ApplyMigrations(scope.ServiceProvider);
                }
                else
                {
                    Console.WriteLine($"Migrations already applied for {migrator.GetType().Name}");
                }
            }
        }
    }
    public static List<ModulesManifest> LoadModulesFromSubdirectories(string modulesRootPath)
    {
        var modules = new List<ModulesManifest>();

        var jsonFiles = Directory.GetFiles(modulesRootPath, "*.json", SearchOption.AllDirectories);

        foreach (var file in jsonFiles)
        {
            try
            {
                string json = File.ReadAllText(file);
                var module = JsonSerializer.Deserialize<ModulesManifest>(json);
                if (module != null)
                    modules.Add(module);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file '{file}': {ex.Message}");
            }
        }

        return modules;
    }
}
