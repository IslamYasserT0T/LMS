using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SampleModule.Infrastructure.Presistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Infrastructure.Presistence;

public class SampleModuleDbContextFactory : IDesignTimeDbContextFactory<SampleModuleDBContext>
{

    SampleModuleDBContext IDesignTimeDbContextFactory<SampleModuleDBContext>.CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SampleModuleDBContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=LMSLocal;Username=postgres;Password=1234;Include Error Detail=true");

        return new SampleModuleDBContext(optionsBuilder.Options);
    }
}
