using Microsoft.EntityFrameworkCore;
using SampleModule.Infrastructure.Presistence.Context;
using SampleModule.Test.TestSetup.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Test;

public class SampleModuleContextGenerator
{
    /// <summary>
    /// Holds a singleton instance of the in-memory database context.
    /// </summary>
    private static SampleModuleDBContext Context;

    /// <summary>
    /// Generates a new instance of <see cref="SampleModuleDBContext"/> if not already created.
    /// Ensures the database is deleted and recreated, then populates it with test data for inventory items.
    /// </summary>
    /// <returns>An instance of <see cref="SampleModuleDBContext"/> with test data.</returns>
    public static SampleModuleDBContext Generator()
    {
        if (Context == null)
        {
            var options = new DbContextOptionsBuilder<SampleModuleDBContext>()
               .UseInMemoryDatabase(databaseName: "samplemodule")

               .Options;

            Context = new SampleModuleDBContext(options);
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
            Context.AddSample();
            Context.SaveChanges();
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        return Context;
    }
}
