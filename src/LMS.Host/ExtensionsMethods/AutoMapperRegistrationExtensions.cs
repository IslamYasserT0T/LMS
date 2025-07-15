using AutoMapper;
using Shared.Application.Mapping;
using System.Reflection;

namespace LMS.Host.ExtensionsMethods;

public static class AutoMapperRegistrationExtensions
{
    public static IServiceCollection AddModuleAutoMappers(this IServiceCollection services, IEnumerable<Assembly> moduleAssemblies)
    {
        var profiles = new List<Profile>();

        foreach (var assembly in moduleAssemblies)
        {
            var providers = assembly.GetTypes()
                .Where(t => typeof(IHasAutoMapperProfiles).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IHasAutoMapperProfiles>();

            foreach (var provider in providers)
            {
                profiles.AddRange(provider.GetProfiles());
            }
        }

        services.AddAutoMapper(cfg =>
        {
            foreach (var profile in profiles)
            {
                cfg.AddProfile(profile);
            }
        });

        return services;
    }
}