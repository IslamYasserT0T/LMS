using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementModule.Application.Extensions;

public static class UserModuleApplicationServiceCollections
{
    public static IServiceCollection AddApplicationServiceCollections(this IServiceCollection services)
    {
        return services;
    }
}
