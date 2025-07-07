using LMS.Host.Components;
using Microsoft.AspNetCore.Builder;
using ModulePoc.Host.Pages;
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

        ModuleManager.LoadModules(_builder.Services);

        _builder.Services.AddControllers();

        _builder.Services.AddRazorComponents()
               .AddInteractiveServerComponents();
        _builder.Services.AddServerSideBlazor();
        _builder.Services.AddMudServices();

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
        app.UseRouting();
        app.UseAntiforgery();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.MapStaticAssets();
        app.UseAuthorization();
        app.MapControllers();
        // app.MapBlazorHub();
        app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(ModuleManager.GetModules().Select(m => m.Assembly).ToArray());
     
        //app.MapBlazorHub();
    }
   
}
