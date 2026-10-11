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
using Microsoft.AspNetCore.Routing;
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
    public IModuleManager ModuleManager { get; }

    public Bootstrapper(WebApplicationBuilder builder, IModuleManager? moduleManager = null)
    {
        _builder = builder;
        ModuleManager = moduleManager ?? new ModuleManager();
    }

    public async Task PreInitialize()
    {
#if DEBUG
        const string activeConfig = "Debug";
#else
        const string activeConfig = "Release";
#endif
        var filesCopier = new ModuleFolderCopier();
        var moduleManifestManager = new ModuleManifestsManager();
        var curDir = Directory.GetCurrentDirectory();
        var dirInfo = new DirectoryInfo(curDir);
        while (dirInfo != null && !File.Exists(Path.Combine(dirInfo.FullName, "LMS.sln")))
        {
            dirInfo = dirInfo.Parent;
        }
        var solutionRoot = dirInfo?.FullName ?? curDir;
        var manifestSource = Path.Combine(solutionRoot, "ManifestPlugins");
        var hostDir = Path.Combine(solutionRoot, "src", "LMS.Host");
        var manifestDest = Directory.Exists(hostDir) ? Path.Combine(hostDir, "manifest") : Path.Combine(curDir, "manifest");

        moduleManifestManager.ProcessManifests(manifestSource, manifestDest, activeConfig);

        var destinationRoot = Path.Combine(AppContext.BaseDirectory, "plugins");
        if (!Directory.Exists(destinationRoot)) 
            Directory.CreateDirectory(destinationRoot);
        await ModuleFolderCopier.CopyFoldersFromManifestAsync(manifestDest, destinationRoot, activeConfig);

        _builder.Services.AddDbContext<BaseDbContext>(options =>
        {
            var connectionString = _builder.Configuration.GetConnectionString("Default");
            if (string.Equals(_builder.Configuration["IntegrationTest:DatabaseProvider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(connectionString);
            else
                options.UseNpgsql(connectionString);
        });

        _builder.Services.AddAuthentication();
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
        _builder.Services.AddCascadingAuthenticationState();
        _builder.Services.AddServerSideBlazor();
        _builder.Services.AddMudServices();
        _builder.Services.AddLocalization();
        _builder.Services.AddSingleton<IModuleManager>(ModuleManager);

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
        await moduleManager.LoadModules(_builder.Services, _builder.Configuration, _builder.Environment);
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

    public async Task ConfigureApplication(WebApplication app, IModuleManager moduleManager)
    {
        ApplyMigrations(app);

        await moduleManager.InitializeRuntimeAsync(app.Services);

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

        var supportedCultures = new[] { "en-US", "en", "ar-SA", "ar" };
        var locOptions = new RequestLocalizationOptions()
            .SetDefaultCulture("en-US")
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
        app.UseRequestLocalization(locOptions);

        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Serve plugin static assets from AppContext.BaseDirectory/plugins/*/wwwroot
        var pluginsRoot = Path.Combine(AppContext.BaseDirectory, "plugins");
        if (Directory.Exists(pluginsRoot))
        {
            foreach (var pluginDir in Directory.GetDirectories(pluginsRoot))
            {
                var pluginWwwroot = Path.Combine(pluginDir, "wwwroot");
                if (Directory.Exists(pluginWwwroot))
                {
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(pluginWwwroot),
                        RequestPath = ""
                    });
                }
            }
        }

        app.MapStaticAssets();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapControllers();

        // Allow modules to register their own endpoints (e.g., IY.Auth token/authorize endpoints).
        foreach (var endpointBuilder in app.Services.GetServices<Action<IEndpointRouteBuilder>>())
        {
            endpointBuilder(app);
        }

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
