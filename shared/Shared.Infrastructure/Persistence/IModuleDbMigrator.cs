using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Persistence;

public interface IModuleDbMigrator
{
    void ApplyMigrations(IServiceProvider serviceProvider);
    bool IsMigrationApplied(IServiceProvider serviceProvider);

}
