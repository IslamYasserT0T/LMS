using FluentValidation;
using System.Reflection;

namespace LMS.Host.ExtensionsMethods;

public static class FluentValidationRegistrationExtensions
{
    public static IServiceCollection AddModuleValidators(this IServiceCollection services, IEnumerable<Assembly> moduleAssemblies)
    {

        foreach (var assembly in moduleAssemblies)
        {

            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        }

        return services;
    }
}
