using Microsoft.EntityFrameworkCore;
using SettingModule.Entities;
using SettingModule.Presistence.Configurations;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingModule.Presistence.Context;
public class SettingDbContext : BaseDbContext
{
    public SettingDbContext(DbContextOptions<SettingDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SettingDefinationConfiguration());
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<SettingDefinition> SettingDefinitions { get; set; }
}
