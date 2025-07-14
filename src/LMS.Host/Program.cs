using LMS.Host.Components;
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
      
            // Configure services
            var bootstrapper = new Bootstrapper(builder);

           await bootstrapper.PreInitialize();
           await bootstrapper.Initialize();
           await bootstrapper.PostInitialize();

            var app = builder.Build();

            // Configure application
            bootstrapper.ConfigureApplication(app);

            app.Run();
        }
    }
}
