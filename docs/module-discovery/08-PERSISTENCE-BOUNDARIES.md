# LMS-D08 — Persistence and Migration Boundaries

**Workstream:** LMS-D08  
**Status:** Confirmed implementation — fully traced.

---

## 1. Persistence Architecture Overview

LMS uses a **per-module DbContext** pattern. Each module that requires persistence defines its own `DbContext`, its own EF Core migrations, and its own `IModuleDbMigrator` implementation. The host provides a `BaseDbContext` as a common base class but does not own entity configuration.

---

## 2. Shared Infrastructure

### BaseDbContext
**File:** `shared/Shared.Infrastructure/Persistence/BaseDbContext.cs`

```csharp
public class BaseDbContext : DbContext, IBaseDbContext
{
    public BaseDbContext(DbContextOptions<BaseDbContext> options) : base(options) { }
    protected BaseDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }
}
```

- A thin base class extending EF Core's `DbContext`.
- Registered in the host with `AddDbContext<BaseDbContext>` (Bootstrapper.cs L64-71).
- Supports both PostgreSQL (Npgsql) and SQLite (for integration tests).
- `IBaseDbContext` interface is also defined; modules register their concrete DbContext as `IBaseDbContext` implementation.

### IModuleDbMigrator
**File:** `shared/Shared.Infrastructure/Persistence/IModuleDbMigrator.cs`

```csharp
public interface IModuleDbMigrator
{
    void ApplyMigrations(IServiceProvider serviceProvider);
    bool IsMigrationApplied(IServiceProvider serviceProvider);
}
```

The host discovers all `IModuleDbMigrator` implementations via `GetServices<IModuleDbMigrator>()` and runs them sequentially during `Bootstrapper.ConfigureApplication` before the middleware pipeline starts:

```csharp
// Bootstrapper.cs L199-215
using (var scope = app.Services.CreateScope())
{
    var migrators = scope.ServiceProvider.GetServices<IModuleDbMigrator>();
    foreach (var migrator in migrators)
    {
        if (!migrator.IsMigrationApplied(scope.ServiceProvider))
            migrator.ApplyMigrations(scope.ServiceProvider);
        else
            Console.WriteLine($"Migrations already applied for {migrator.GetType().Name}");
    }
}
```

---

## 3. Per-Module Persistence Configuration

### SampleModule

| Component | File | Notes |
|---|---|---|
| `SampleModuleDBContext` | `SampleModule.Infrastructure/Presistence/Context/` | Own DbContext, inherits BaseDbContext |
| `SampleModuleMigrator` | `SampleModule.Infrastructure/Presistence/SampleModuleMigrator.cs` | IModuleDbMigrator impl |
| EF Core migrations | `SampleModule.Infrastructure/Presistence/Migrations/` | Module-owned |
| `SampleModuleDbContextFactory` | `SampleModule.Infrastructure/Presistence/SampleModuleDbContextFactory.cs` | For EF CLI migration tools |

**Registration in DI:**
```csharp
// SampleModuleServiceCollections.cs L22-32
services.AddDbContext<SampleModuleDBContext>(options => { ... PostgreSQL or SQLite });
services.AddScoped<IBaseDbContext, SampleModuleDBContext>();
services.AddScoped<ISampleRepository, SampleRepository>();
services.AddScoped<IModuleDbMigrator, SampleModuleMigrator>();
```

**Migration strategy:**
```csharp
// SampleModuleMigrator.cs
public void ApplyMigrations(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SampleModuleDBContext>();
    if (context.Database.IsSqlite())
    {
        SqliteTestDatabaseInitializer.EnsureInitialized(context, "Samples");
        return;
    }
    context.Database.Migrate();   // EF Core standard migration
}
```

SQLite path uses a test-specific initializer (creates tables directly without migrations). PostgreSQL uses standard EF Core `Database.Migrate()`.

### IdentityModule

| Component | File | Notes |
|---|---|---|
| `IdentityDbContext` | Provided by `IY.Auth.EntityFrameworkCore` NuGet | External |
| `IdentityModuleDbMigrator` | `LMS.IdentityModule/IdentityModuleDbMigrator.cs` | IModuleDbMigrator impl |

The IdentityModule delegates migrations to `IIdentityDatabaseMigrator` from the IY.Auth package. The migrator is obtained from DI.

Schema isolation: configures a dedicated PostgreSQL schema via:
```csharp
npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema);
```
where `schema` defaults to `"identity"` from configuration.

### AuthorizationModule

| Component | File | Notes |
|---|---|---|
| `AuthorizationDbContext` | `AuthorizationModule/Presistence/Context/` | Own DbContext |
| `AuthorizeModuleMigrator` | `AuthorizationModule/Presistence/` | IModuleDbMigrator impl |

Registered in `AuthorizationModuleServiceCollectionExtensions.cs` L47:
```csharp
services.AddScoped<IModuleDbMigrator, AuthorizeModuleMigrator>();
```

---

## 4. Database Connection Sharing

All modules share the same connection string: `ConnectionStrings:Default`. There is no per-module connection string. Isolation is achieved through:
- Schema separation (IdentityModule uses `identity` schema)
- Separate DbContext registrations (no entity set collisions at the ORM level)

This means all modules write to the **same database** with implicit schema-level isolation. There is no physical database isolation between modules.

---

## 5. SqliteTestDatabaseInitializer

**File:** `shared/Shared.Infrastructure/Persistence/SqliteTestDatabaseInitializer.cs`

A test utility that creates tables directly in an SQLite database when EF Core migrations are not available/appropriate. Used by both `SampleModuleMigrator` and `IdentityModuleDbMigrator` in test scenarios. Not used in production PostgreSQL path.

---

## 6. Unit of Work / Transactions

No shared unit-of-work pattern was found. Each module manages transactions through its own DbContext. There is no cross-module transaction support.

---

## 7. Repository Pattern

Each module defines its own repository interfaces (e.g., `ISampleRepository`) and implementations. Repositories are registered scoped. No shared generic repository base found in the inspected scope.

---

## 8. Coupling That Would Prevent Independent Reuse

| Coupling | Impact |
|---|---|
| All modules share `ConnectionStrings:Default` | Cannot run modules against isolated databases without configuration changes |
| `SampleModuleDBContext` inherits `BaseDbContext` (from Shared.Infrastructure) | Extracting SampleModule requires Shared.Infrastructure |
| `AuthorizationModule` referenced from `Shared.Core.csproj` | SettingModule and AuthorizationModule are required by all consumers of Shared.Core |
| `IModuleDbMigrator` from Shared.Infrastructure | Any module that owns persistence must reference Shared.Infrastructure |
| Migration runner uses a single scope — no parallel migration | Multi-module migration is sequential, blocking |

---

## 9. Migration Ordering

Migrators are ordered by DI registration order. Since modules are registered in filesystem order (non-deterministic), migration order between modules is not guaranteed. Each module uses its own schema/migrations table, so ordering only matters if there are cross-module foreign keys — none were found in the inspected code.

---

## 10. Do Not Run Migrations

Per mission requirements: **no migrations were executed**. All observations are based on source code inspection only.
