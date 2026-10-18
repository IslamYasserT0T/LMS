# IY.Modules — Architecture Handoff Document

**Prepared by:** Phase A Discovery Mission  
**Date:** 2026-10-13  
**Source workspace:** `d:\LMS` (LMS.sln, net9.0, Visual Studio 17)  
**Purpose:** This document is self-contained. It can be used in a separate IY.Modules workspace without access to the LMS repository. All source references use relative paths within LMS for traceability.

> No credentials, secrets, connection strings, or business data are included in this document.

---

## 1. Existing LMS Module Architecture

### 1.1 Overview

LMS implements a **manifest-driven, file-system plugin loader**. Modules are independent class libraries compiled and published separately. At application startup, the host reads JSON manifests to discover which plugin directories to load, copies the plugin output into a centralized `plugins/` folder, then loads each plugin DLL using `Assembly.LoadFrom` into the default `AssemblyLoadContext`.

### 1.2 Physical Layout

```
LMS.sln root
├── src/LMS.Host/           Host: entry point, orchestrator
│   ├── Services/
│   │   ├── Bootstrapper.cs           Four-phase startup coordinator
│   │   ├── ModuleManager.cs          Plugin discovery, loading, service wiring
│   │   ├── ModuleManifestsManager.cs Manifest merge + token substitution
│   │   ├── ModuleFolderCopier.cs     File-system deployment copier
│   │   └── ModuleInfo.cs             Runtime module descriptor
│   ├── Contracts/IModuleManager.cs   Host orchestration contract
│   └── manifest                      Merged JSON manifest (runtime artifact)
│
├── shared/
│   ├── Shared.Core/Contracts/
│   │   ├── IModule.cs                Primary plugin contract
│   │   ├── IUiModule.cs              Blazor UI marker
│   │   └── IData.cs                  Empty marker
│   ├── Shared.Core/DependencyInjections/
│   │   ├── IApplicationService.cs    Scannable service marker
│   │   ├── IScopedService.cs
│   │   ├── ISingletonService.cs
│   │   └── ITransientService.cs
│   ├── Shared.Infrastructure/Persistence/
│   │   ├── BaseDbContext.cs          EF Core base context
│   │   ├── IModuleDbMigrator.cs      Migration contract
│   │   └── SqliteTestDatabaseInitializer.cs
│   ├── Modules/SettingModule/        Built-in (static) setting module
│   └── Modules/AuthorizationModule/  Built-in (static) Casbin RBAC module
│
├── modules/                           Plugin module projects
│   ├── SampleModule/                  Multi-assembly: API, Application, Core, Domain, Infrastructure, Ui
│   ├── IdentityModule/LMS.IdentityModule/  Single-assembly, IY.Auth/OpenIddict
│   └── ReportingModule/LMS.ReportingModule/  Single-assembly, IY.Reporting/SqlKata
│
└── ManifestPlugins/                   Per-module JSON manifests
    ├── SampleModuleMinifest.json
    ├── IdentityModuleManifest.json
    └── ReportingModuleManifest.json
```

### 1.3 Startup Sequence (Exact)

```
Program.Main(args)
  1. WebApplication.CreateBuilder(args)
  2. new Bootstrapper(builder, new ModuleManager())
  3. bootstrapper.PreInitialize()          ← host infrastructure + manifest merge + file copy
  4. bootstrapper.Initialize(manager)      ← Assembly.LoadFrom + IModule.RegisterServices per plugin
  5. bootstrapper.PostInitialize(manager)  ← AutoMapper, FluentValidation, Razor parts
  6. builder.Build()                       ← service provider locked
  7. bootstrapper.ConfigureApplication(app, manager)
       ├── ApplyMigrations(app)            ← IModuleDbMigrator per module
       ├── manager.InitializeRuntimeAsync(app.Services)
       │     ├── ExecuteSettingProviders   ← ISettingDefinationProvider scan + Define()
       │     └── ExecuteNavProviders       ← INavigationProvider GetMenuItems()
       └── HTTP pipeline + endpoint registration
  8. app.Run()
```

### 1.4 Module Contract (Exact Source)

```csharp
// shared/Shared.Core/Contracts/IModule.cs
namespace Shared.Core.Contracts;
public interface IModule
{
    string Name { get; }
    string Description { get; }
    Type ModuleType { get; }
    bool IsEnabled { get; }
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
    void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment? hostEnvironment)
    {
        RegisterServices(services, configuration);
    }
}
```

`IsEnabled` is **not enforced** — the loader never reads it to skip modules.  
`ModuleType` is used only to detect `IUiModule` — its actual value is arbitrary.

### 1.5 Manifest JSON Schema

```json
// ManifestPlugins/SampleModuleMinifest.json
{
  "ModuleName": "SampleModule",
  "Modules": [
    {
      "Name": "SampleModule.API",
      "Version": "1.0.0",
      "CopyFolder": "modules/SampleModule/SampleModule.API/bin/{Configuration}/net9.0/plugins"
    }
  ]
}
```

`{Configuration}` token is substituted with `Debug` or `Release` at startup. Multiple manifests are merged into a single `ModulesRoot` JSON.

### 1.6 Module Loading Logic (Exact)

```csharp
// ModuleManager.cs — simplified
var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
foreach (var dir in Directory.GetDirectories(pluginsDir))
{
    var dirName = Path.GetFileName(dir);
    var pluginDll = Path.Combine(dir, dirName + ".dll");
    if (!File.Exists(pluginDll)) continue;

    try
    {
        var assembly = Assembly.LoadFrom(pluginDll);  // default ALC, no isolation
        var moduleTypes = assembly.GetTypes()
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        foreach (var moduleType in moduleTypes)
        {
            if (ModuleInfo.TryCreateModule(assembly, out var moduleInfo))
                RegisterModuleServices(moduleType, moduleInfo);
        }

        LoadCompanionBlazorAssemblies(dir);  // *.UI.dll, *.Blazor.dll
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load module from {pluginDll}: {ex.Message}");
        // Silent failure — host continues without this module
    }
}
```

---

## 2. Confirmed Reusable Components

The following can be extracted into IY.Modules NuGet packages with namespace changes only:

| Component | Source Path | Target Package |
|---|---|---|
| `IModule` interface | `shared/Shared.Core/Contracts/IModule.cs` | `IY.Modules.Abstractions` |
| `IUiModule` interface | `shared/Shared.Core/Contracts/IUiModule.cs` | `IY.Modules.Abstractions` |
| `IApplicationService` | `shared/Shared.Core/DependencyInjections/` | `IY.Modules.Abstractions` |
| `IScopedService` / `ISingletonService` / `ITransientService` | same | `IY.Modules.Abstractions` |
| `IModuleDbMigrator` | `shared/Shared.Infrastructure/Persistence/IModuleDbMigrator.cs` | `IY.Modules.Abstractions.Persistence` |
| `ModuleFolderCopier` | `src/LMS.Host/Services/ModuleFolderCopier.cs` | `IY.Modules.Deployment` |
| `ModuleManifestsManager` | `src/LMS.Host/Services/ModuleManifestsManager.cs` | `IY.Modules.Deployment` |
| Manifest DTOs | `src/LMS.Host/Dtos/` | `IY.Modules.Deployment` |
| `ServiceRepository` scan pattern | `SampleModule.Infrastructure/Extensions/ServiceRepository.cs` | `IY.Modules.Hosting` |

---

## 3. Components Requiring Redesign

| Component | Current Problem | Required Redesign |
|---|---|---|
| Assembly loading (`Assembly.LoadFrom`) | Default ALC, no isolation | Per-module `AssemblyLoadContext` |
| Module ordering | Filesystem order (non-deterministic) | Topological sort on declared Dependencies |
| Failure handling | `catch + Console.WriteLine` | Configurable failure policy + `ILogger` + degraded health |
| Module lifecycle | Only `RegisterServices` | Multi-phase: `RegisterServices` / `Initialize` / `Start` / `Stop` / `Dispose` |
| `ModuleManager` coupling | Hard-wired to `SettingDbContext` | Generic `IModuleStartupHook` / `IModuleInitializer` |
| `IModuleManager` interface | Returns Blazor-specific types | Separate into core `IModuleHost` + Blazor extension |
| `Bootstrapper` | Mixes host wiring with module orchestration | Extract orchestration interfaces; keep wiring in host |
| Runtime management | None | State machine: Discovered / Registered / Started / Failed / Stopped |

---

## 4. Module Lifecycle Requirements

Based on what is currently missing and what Orchard Core provides as reference:

### Minimum viable lifecycle for IY.Modules v1

```csharp
public interface IModule
{
    // Identity
    string Id { get; }            // stable, unique key (not display name)
    string Name { get; }
    string Version { get; }
    string Description { get; }
    bool IsEnabled { get; }       // MUST be enforced by loader

    // Declared inter-module dependencies
    IReadOnlyList<string> Dependencies { get; }

    // Startup
    void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment? hostEnvironment);
}

public interface IModuleLifecycle  // optional, implemented per module
{
    Task OnStartingAsync(IServiceProvider services, CancellationToken cancellationToken);
    Task OnStartedAsync(IServiceProvider services, CancellationToken cancellationToken);
    Task OnStoppingAsync(CancellationToken cancellationToken);
}

public interface IModulePipelineContributor  // optional
{
    void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes);
}
```

### Required module states

```
NotDiscovered → Discovered → DependenciesResolved → ServicesRegistered → Started → Stopped
                                                                       ↘ Failed (quarantined)
```

---

## 5. Dependency Injection Constraints

- **Single shared container**: LMS uses one `IServiceCollection`. IY.Modules v1 should maintain this for compatibility.
- **Registration window**: Must happen before `IHost.Build()` / `WebApplication.Build()`. No post-startup DI registration.
- **Ordering matters**: `EnsureSatelliteAssemblyResolverRegistered()` must be called before any `Assembly.LoadFrom`. Similarly, IY.Modules must pre-register type resolvers before loading begins.
- **Duplicate-safe**: Host may call `AddControllers()`, `AddLocalization()`, etc. before modules do. Registrations must be idempotent.
- **`IModuleDbMigrator` scoped**: Migration runners are scoped services — they must be resolved in a scope, not from root provider.
- **Anti-pattern**: `IServiceCollection` must not be stored as a field after `Build()`. `ModuleManager` currently does this (L20) — do not replicate.

---

## 6. Assembly Loading Constraints

- **LMS compatibility (v1)**: All modules share the default ALC → no cross-ALC type casting needed. `IModule` from plugin is the same `IModule` as the host knows.
- **v1 recommendation**: Non-collectible per-module `AssemblyLoadContext` where the module's own DLLs are private but shared libraries (the `IY.Modules.Abstractions` NuGet) are resolved from the parent (default) ALC.
- **Satellite assembly pattern**: The existing `AssemblyResolve` handler (ModuleManager.cs L247-286) must be ported — it supports `ar`, `ar-SA`, `en`, `en-US` resource DLLs in plugin subdirectories.
- **Companion Blazor assemblies**: `.UI.dll` and `.Blazor.dll` conventions must be preserved or a configuration-based alternative provided.
- **Version pinning**: `Microsoft.Extensions.DependencyInjection.Abstractions` must be net9.0 / 9.x — the regression test suite (PluginDeploymentRegressionTests.cs) enforces this.
- **`Assembly.LoadFrom` avoidance**: Do not use `Assembly.LoadFrom` in IY.Modules — use `AssemblyLoadContext.LoadFromAssemblyPath` instead.

---

## 7. ASP.NET Core Integration Requirements

- **Controllers**: Modules register application parts via `services.AddControllers().AddApplicationPart(assembly)`. IY.Modules must invoke this at the right time (before `Build()`).
- **Minimal API endpoints**: Use the `Action<IEndpointRouteBuilder>` delegate pattern (IdentityModule). IY.Modules should discover and invoke all registered `Action<IEndpointRouteBuilder>` singletons in `ConfigureApplication`.
- **Middleware**: LMS has no module middleware. IY.Modules should support `IModulePipelineContributor.Configure(app, routes)` called in `ConfigureApplication`.
- **Blazor pages**: Discovered route-bearing assemblies must be passed to `app.MapRazorComponents<App>().AddAdditionalAssemblies(...)`. IY.Modules should provide `GetBlazorAssemblies()` method.
- **Static files per plugin**: Each `plugins/<Name>/wwwroot` should be mounted as a `PhysicalFileProvider` with `RequestPath = ""`.
- **Endpoint registration timing**: Must happen after `app.UseRouting()` and `app.UseAuthentication()` but before `app.Run()`.
- **Migration timing**: `IModuleDbMigrator` implementations run after `Build()` but before the HTTP pipeline starts accepting requests.

---

## 8. Security and Persistence Constraints

### Security
- The IdentityModule depends on `IY.Auth` (private NuGet). IY.Modules must not reference this package — the module is self-contained.
- `Action<IEndpointRouteBuilder>` pattern decouples the IY.Auth endpoint registration from the host — **preserve this pattern**.
- `Reporting:EnableIntegrationProof` is a security gating flag — proof harness types must be excluded from Release builds. IY.Modules does not need to enforce this; it is module-internal.
- Casbin policy file is at `wwwroot/plugins/AuthorizationModule/casbin_model.conf` — do not change this path.
- Module management APIs (future) must be protected by strong authorization.

### Persistence
- All modules share `ConnectionStrings:Default`.
- Each module uses its own `DbContext` derived from `BaseDbContext`.
- Migrations run sequentially per `IModuleDbMigrator` registration order.
- The `IModuleDbMigrator` interface is the **only** persistence seam between IY.Modules and module data layers.
- IY.Modules should not own a database — it is a hosting framework, not a persistence framework.
- SQLite integration test path (`IntegrationTest:DatabaseProvider = "Sqlite"`) must be preserved in all module DbContext configurations.

---

## 9. Compatibility Requirements

### Must preserve (LMS backward compatibility)
1. `IModule.RegisterServices(IServiceCollection, IConfiguration)` — two-parameter overload
2. `IModule.RegisterServices(IServiceCollection, IConfiguration, IHostEnvironment?)` — three-parameter overload with default implementation
3. `IModuleDbMigrator.ApplyMigrations(IServiceProvider)` / `IsMigrationApplied(IServiceProvider)`
4. `plugins/<Name>/<Name>.dll` discovery convention
5. JSON manifest schema (`ModuleName`, `Modules[].Name`, `Modules[].Version`, `Modules[].CopyFolder`)
6. `{Configuration}` token substitution in manifest `CopyFolder`
7. Satellite assembly resolution for `ar`, `ar-SA`, `en`, `en-US` culture paths
8. `.UI.dll` / `.Blazor.dll` companion assembly convention
9. `plugins/<Name>/wwwroot` static file mounting
10. `Action<IEndpointRouteBuilder>` singleton pattern for module endpoint registration

### Breaking changes acceptable for IY.Modules (new framework)
- New `IModule.Id` property (modules must implement it, but can return `Name` for backward compat)
- New `IModule.Dependencies` property (can return empty list for backward compat)
- New `IModuleLifecycle` interface (optional, not required for existing modules)

---

## 10. Recommended Implementation Priorities

### Phase 1 — Abstractions Package
- `IY.Modules.Abstractions` NuGet: `IModule`, `IUiModule`, `IModuleDbMigrator`, DI markers
- No dependencies except `Microsoft.Extensions.Hosting.Abstractions` and `Microsoft.Extensions.DependencyInjection.Abstractions`
- Target: `netstandard2.0` or `net9.0`

### Phase 2 — Deployment Package
- `IY.Modules.Deployment` NuGet: `ModuleFolderCopier`, `ModuleManifestsManager`, manifest DTOs
- No runtime dependencies (only `System.Text.Json`)

### Phase 3 — Hosting Package
- `IY.Modules.Hosting` NuGet: `ModuleLoader` (using AssemblyLoadContext), topological sort, failure policy, `ILogger` integration
- Depends on `IY.Modules.Abstractions`, `IY.Modules.Deployment`, `Microsoft.Extensions.Logging.Abstractions`

### Phase 4 — ASP.NET Core Integration Package
- `IY.Modules.AspNetCore` NuGet: `WebApplicationBuilderExtensions`, `IModulePipelineContributor`, Blazor assembly discovery
- Depends on `IY.Modules.Hosting`, `Microsoft.AspNetCore.App`

### Phase 5 — LMS Adapter
- Update `LMS.Host` to use `IY.Modules.AspNetCore` instead of the internal `Bootstrapper` / `ModuleManager`
- No changes to individual module projects

---

## 11. Unresolved Architectural Decisions

These decisions must be made before implementation begins:

| Decision | Options | Recommendation |
|---|---|---|
| Assembly isolation level | (a) Default ALC (LMS-compatible), (b) Per-module non-collectible ALC | (b) Per-module — better long-term; use shared-type resolution for contracts |
| Module dependency declaration | (a) Manifest JSON `Dependencies[]`, (b) C# attribute `[ModuleDependency("x")]`, (c) Both | (a) JSON manifest — tooling-friendly, no code change to modules |
| Failure policy | (a) Fatal, (b) Quarantine, (c) Warn (current) | Configurable per module, default Quarantine |
| Lifecycle hooks | (a) Separate `IModuleLifecycle` interface, (b) Additional methods on `IModule` | (a) Separate interface — optional, backward compat |
| Runtime activation | (a) Not in v1 (restart required), (b) Shell-restart per Orchard pattern, (c) True hot-swap | (a) v1 — restart required. Document v2 path. |
| Middleware contribution | (a) Not in v1, (b) `IModulePipelineContributor` | (b) Relatively low-cost, high value |
| Migration ownership | (a) IY.Modules orchestrates all `IModuleDbMigrator`, (b) Host owns | (a) IY.Modules orchestrates — cleaner module boundary |
| Setting integration | (a) Keep `ISettingDefinationProvider` coupling, (b) Generic `IModuleInitializer` | (b) Generic — IY.Modules should not know about settings |
| Admin API | (a) None in v1, (b) Minimal read-only diagnostics endpoint | (b) Minimal diagnostics only in v1 |
| Blazor UI coupling | (a) Always in hosting package, (b) Separate `IY.Modules.Blazor` package | (b) Separate — not all IY.Modules consumers use Blazor |

---

## 12. Source References

| Finding | File | Line |
|---|---|---|
| Startup sequence | `src/LMS.Host/Program.cs` | 13-31 |
| PreInitialize | `src/LMS.Host/Services/Bootstrapper.cs` | 37-105 |
| Initialize | `src/LMS.Host/Services/Bootstrapper.cs` | 107-110 |
| PostInitialize | `src/LMS.Host/Services/Bootstrapper.cs` | 112-124 |
| ConfigureApplication | `src/LMS.Host/Services/Bootstrapper.cs` | 126-196 |
| ApplyMigrations | `src/LMS.Host/Services/Bootstrapper.cs` | 198-216 |
| LoadModule (core loop) | `src/LMS.Host/Services/ModuleManager.cs` | 123-170 |
| Assembly.LoadFrom | `src/LMS.Host/Services/ModuleManager.cs` | 137 |
| IModule type scan | `src/LMS.Host/Services/ModuleManager.cs` | 139-141 |
| RegisterModuleServices | `src/LMS.Host/Services/ModuleManager.cs` | 232-244 |
| Silent failure | `src/LMS.Host/Services/ModuleManager.cs` | 155-161 |
| ExecuteSettingProviders | `src/LMS.Host/Services/ModuleManager.cs` | 81-115 |
| SettingDbContext coupling | `src/LMS.Host/Services/ModuleManager.cs` | 84 |
| Satellite resolver | `src/LMS.Host/Services/ModuleManager.cs` | 247-286 |
| Companion Blazor discovery | `src/LMS.Host/Services/ModuleManager.cs` | 178-216 |
| Endpoint delegate pattern | `modules/IdentityModule/LMS.IdentityModule/IdentityModuleServiceCollectionExtensions.cs` | 47 |
| Endpoint invocation | `src/LMS.Host/Services/Bootstrapper.cs` | 184-187 |
| Blazor additional assemblies | `src/LMS.Host/Services/Bootstrapper.cs` | 189-195 |
| Plugin wwwroot mounting | `src/LMS.Host/Services/Bootstrapper.cs` | 160-175 |
| IModule contract | `shared/Shared.Core/Contracts/IModule.cs` | 11-22 |
| IUiModule marker | `shared/Shared.Core/Contracts/IUiModule.cs` | 9-12 |
| IModuleDbMigrator | `shared/Shared.Infrastructure/Persistence/IModuleDbMigrator.cs` | 9-14 |
| BaseDbContext | `shared/Shared.Infrastructure/Persistence/BaseDbContext.cs` | 13-21 |
| Shared.Core → SettingModule coupling | `shared/Shared.Core/Shared.Core.csproj` | 17-18 |
| SampleModule DI | `modules/SampleModule/SampleModule.Infrastructure/Extensions/SampleModuleServiceCollections.cs` | 20-34 |
| ServiceRepository scan | `modules/SampleModule/SampleModule.Infrastructure/Extensions/ServiceRepository.cs` | 16-33 |
| ISettingDefinationProvider | `shared/Modules/SettingModule/Contracts/ISettingDefinationProvider.cs` | 2-5 |
| Release/Debug guard | `src/LMS.Host/Services/ModuleFolderCopier.cs` | 44-51 |
| Manifest merge | `src/LMS.Host/Services/ModuleManifestsManager.cs` | 8-18 |
| Token substitution | `src/LMS.Host/Services/ModuleManifestsManager.cs` | 55-56 |
| Test: path traversal protection | `modules/ReportingModule/LMS.ReportingModule.Test/ModuleFolderCopierTests.cs` | 85-97 |
| Test: security isolation | `modules/ReportingModule/LMS.ReportingModule.Test/SecurityIsolationTests.cs` | all |
| Test: component verification | `modules/ReportingModule/LMS.ReportingModule.Test/ComponentVerificationTests.cs` | all |
| Test: version regression | `modules/ReportingModule/LMS.ReportingModule.Test/PluginDeploymentRegressionTests.cs` | 115-128 |
