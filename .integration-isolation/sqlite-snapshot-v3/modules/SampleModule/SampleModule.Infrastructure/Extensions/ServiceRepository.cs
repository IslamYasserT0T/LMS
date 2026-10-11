using Microsoft.Extensions.DependencyInjection;
using Shared.Core.DependencyInjections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Infrastructure.Extensions;
/// <summary>
/// Static class for defining services for infrastructure layer.
/// </summary>
public static class ServiceRepository
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
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
