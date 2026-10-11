using Microsoft.Extensions.DependencyInjection;
using SampleModule.Application.Contracts;
using SampleModule.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Application.Extensions;

public static class SampleModuleApplicationServiceCollections
{
    public static IServiceCollection AddSampleModuleApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISampleAppService, SampleAppService>();

        return services;
    }
}
