using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleModule.Infrastructure.Presistence.Context;
using SettingModule.Presistence.Context;
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
        services.AddRepositoryServices();
        return services;
    }
}
