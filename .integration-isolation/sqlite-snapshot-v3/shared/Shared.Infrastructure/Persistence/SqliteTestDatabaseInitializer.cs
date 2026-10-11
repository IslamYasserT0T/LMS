using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Shared.Infrastructure.Persistence;

/// <summary>
/// Initializes one EF model at a time in the opt-in SQLite integration fixture.
/// Production providers keep their migration-based initialization.
/// </summary>
public static class SqliteTestDatabaseInitializer
{
    public static bool IsInitialized(DbContext context, string probeTable)
    {
        if (!string.Equals(context.Database.ProviderName, "Microsoft.EntityFrameworkCore.Sqlite", StringComparison.Ordinal))
            throw new InvalidOperationException("SQLite test initialization was requested for a non-SQLite provider.");

        try
        {
            context.Database.ExecuteSqlRaw($"SELECT 1 FROM \"{probeTable}\" LIMIT 1");
            return true;
        }
        catch (DbException)
        {
            return false;
        }
    }

    public static void EnsureInitialized(DbContext context, string probeTable)
    {
        if (IsInitialized(context, probeTable))
            return;

        if (!context.Database.CanConnect())
        {
            context.Database.EnsureCreated();
            return;
        }

        context.Database.GetService<IRelationalDatabaseCreator>().CreateTables();
    }
}
