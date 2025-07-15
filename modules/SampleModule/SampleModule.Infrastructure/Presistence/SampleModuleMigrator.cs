using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleModule.Infrastructure.Presistence.Context;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Infrastructure.Presistence;

public class SampleModuleMigrator : IModuleDbMigrator
{
    public void ApplyMigrations(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<SampleModuleDBContext>();
        context.Database.Migrate();
    }
}