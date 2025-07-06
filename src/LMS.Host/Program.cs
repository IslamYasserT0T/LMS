using LMS.Host.Components;
using LMS.Host.Services;

namespace LMS.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);
            // Configure services
            var bootstrapper = new Bootstrapper(builder);

            bootstrapper.ConfigureServices();

            var app = builder.Build();

            // Configure application
            bootstrapper.ConfigureApplication(app);

            app.Run();
        }
    }
}
