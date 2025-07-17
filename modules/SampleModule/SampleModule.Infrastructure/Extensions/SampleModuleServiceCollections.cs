using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleModule.Domain.Repositories.Samples;
using SampleModule.Infrastructure.Presistence;
using SampleModule.Infrastructure.Presistence.Context;
using SampleModule.Infrastructure.Repositories.Samples;
using SettingModule.Presistence.Context;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Infrastructure.Extensions;

public static class SampleModuleServiceCollections
{
    public static IServiceCollection AddInfrastructureSampleModuleServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SampleModuleDBContext>(options =>
      options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<ISampleRepository, SampleRepository>();
        services.AddScoped<IModuleDbMigrator, SampleModuleMigrator>();
        return services;
    }
}
