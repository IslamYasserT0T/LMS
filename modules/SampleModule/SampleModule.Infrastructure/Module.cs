using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleModule.Domain.Repositories.Samples;
using SampleModule.Infrastructure.Extensions;
using SampleModule.Infrastructure.Presistence;
using SampleModule.Infrastructure.Presistence.Context;
using SampleModule.Infrastructure.Repositories.Samples;
using Shared.Core.Contracts;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Infrastructure;

public class Module : IModule
{
    public string Description => "The sample module infrastructure";
    public string Name => "Sample Module infrastructure";
    public string NavigationLinkName => " ";
    public string URL => " ";

    public Type ModuleType => typeof(IData);

    public string StaticFilesPath => " ";

    public bool IsEnabled => true;


    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SampleModuleDBContext>(options =>
          options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<ISampleRepository, SampleRepository>();
        services.AddScoped<IModuleDbMigrator, SampleModuleMigrator>();
    }
}
