using Microsoft.Extensions.DependencyInjection;
using SampleModule.Infrastructure.Extensions;
using Shared.Core.DependencyInjections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SampleModule.Application.Extensions;
/// <summary>
/// Static class for defining sample module services
/// </summary>
public static class SampleModuleExtentions
{
    public static IServiceCollection AddApplicationSampleModuleServices(this IServiceCollection services)
    {
        var serviceTypes = Assembly.GetExecutingAssembly().ExportedTypes
            .Where(t => t.IsAssignableTo(typeof(IApplicationService)) && !t.IsInterface && !t.IsAbstract).ToList();

        foreach (var serviceType in serviceTypes)
        {
            var interfaceType = serviceType.GetInterface("I" + serviceType.Name);
            var result = serviceType switch
            {
                _ when typeof(IScopedService).IsAssignableFrom(serviceType) => services.AddScoped(interfaceType, serviceType),
                _ when typeof(ITransientService).IsAssignableFrom(serviceType) => services.AddTransient(interfaceType, serviceType),
                _ when typeof(ISingletonService).IsAssignableFrom(serviceType) => services.AddSingleton(interfaceType, serviceType),
            };
        }

        return services;
    }
}
