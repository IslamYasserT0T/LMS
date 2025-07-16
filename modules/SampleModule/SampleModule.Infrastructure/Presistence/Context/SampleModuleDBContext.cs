using Microsoft.EntityFrameworkCore;
using SampleModule.Domain.Entities;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Infrastructure.Presistence.Context;

public class SampleModuleDBContext : BaseDbContext
{
    public SampleModuleDBContext(DbContextOptions<SampleModuleDBContext> options) : base(options)
    {
    }

    #region DbSets
    public DbSet<Sample> Samples { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleModuleDBContext).Assembly);
    }
}
