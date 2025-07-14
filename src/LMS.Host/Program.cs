using LMS.Host.Components;
using LMS.Host.Contracts;
using LMS.Host.Services;
using Microsoft.EntityFrameworkCore;
using SettingModule.Contracts;
using SettingModule.Services;
using Shared.Infrastructure.Persistence;

namespace LMS.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
      
            var bootstrapper = new Bootstrapper(builder);

           await bootstrapper.PreInitialize();
            var moduleManager = builder.Services.BuildServiceProvider().GetRequiredService<IModuleManager>();

            await bootstrapper.Initialize(moduleManager);
           await bootstrapper.PostInitialize(moduleManager);

            var app = builder.Build();

            bootstrapper.ConfigureApplication(app, moduleManager);

            app.Run();
        }
    }
}
