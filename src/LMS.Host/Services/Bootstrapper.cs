using LMS.Host.Components;
using MudBlazor.Services;

namespace LMS.Host.Services;

public class Bootstrapper
{
    private readonly WebApplicationBuilder _builder;

    public Bootstrapper(WebApplicationBuilder builder)
    {
        _builder = builder;
    }
    public void ConfigureServices()
    {
        _builder.Services.AddServerSideBlazor();
        _builder.Services.AddMudServices();
        _builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        ModuleManager.LoadModules(_builder.Services);

        _builder.Services.AddControllers();

        var mvcBuilder = _builder.Services.AddRazorPages();
           

        foreach (var assembly in ModuleManager.GetUiModules())
        {
            mvcBuilder.AddApplicationPart(assembly.Assembly);
        }
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

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAntiforgery();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        app.MapBlazorHub();
    }
   
}
