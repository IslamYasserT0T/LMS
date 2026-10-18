# LMS-D12 — Orchard Core Comparison

**Workstream:** LMS-D12  
**Reference:** Orchard Core 1.x open-source module/tenant framework.  
**Basis:** Architectural knowledge of Orchard Core; LMS source inspected per this mission.

---

## Comparison Table

| Concept | Orchard Core | LMS | Recommendation |
|---|---|---|---|
| Extension discovery | Directory scan + manifest (`Module.txt`) | Directory scan + JSON manifests | **Adapt** |
| Module metadata | Rich: `Id`, `Name`, `Version`, `Description`, `Tags`, `Dependencies`, `Category` in `Module.txt` | Minimal: `Name`, `Description` in `IModule` properties; version only in manifest JSON | **Adopt conceptually** |
| Feature dependencies | First-class: feature list + depends-on declarations, enforced by topological sort | Not implemented | **Adopt** |
| Application composition | `IModularTenantEvents`, `IStartup` per module | `IModule.RegisterServices` (single method) | **Adapt** |
| Lifecycle hooks | `IModularTenantEvents.ActivatingAsync/ActivatedAsync/TerminatingAsync/TerminatedAsync` | None beyond `RegisterServices` | **Adopt** |
| Tenant isolation | Full multi-tenant via `OrchardCore.Environment.Shell` | Single-tenant only | **Defer** |
| Runtime activation | Tenants can enable/disable features at runtime (requires shell restart per tenant) | Not implemented | **Adopt partially** |
| Service registration | `IStartup.ConfigureServices` (per module startup class) | `IModule.RegisterServices` (equivalent concept) | **Identical pattern — LMS already has this** |
| Module capabilities | Feature-level granularity within a module | Module-level only | **Defer** |
| Assembly loading | `IExtensionLoader` → `AssemblyLoadContext` per extension type | `Assembly.LoadFrom` into default ALC | **Adopt conceptually (ALC per module)** |
| Static files | `IFileProvider` composition (virtual file system) | `PhysicalFileProvider` per plugin wwwroot | **Adapt** |
| Navigation / menus | `INavigationProvider` (registered in DI) | `INavigationProvider` (same concept, same name) | **Already aligned** |
| Settings | `ISiteService` site settings, `ISettingsService` | `ISettingDefinationProvider` (similar concept) | **Adapted version already in LMS** |
| Middleware / pipeline | `IStartup.Configure` per module | Not supported | **Adopt** |
| Authorization | Permission system with `IPermissionProvider` | Casbin RBAC via shared `AuthorizationModule` | **Different approach — LMS is adequate** |
| Database/migrations | Per-module EF Core, `IDbContext` registration | Per-module EF Core, `IModuleDbMigrator` | **LMS pattern is adequate** |
| Background tasks | `IBackgroundTask` / hosted services per module | None | **Adopt** |
| Diagnostics | Structured logging, tracing | `Console.WriteLine` only | **Adopt ILogger** |
| Startup failure | Graceful degradation with logging | Silent (`Console.WriteLine`) | **Adopt explicit failure handling** |
| Admin UI | Full `Orchard Core` dashboard for module management | None | **Design for IY.Modules** |

---

## Detailed Analysis

### Extension Discovery — Adapt

Orchard Core uses `Module.txt` manifest files per module directory. LMS uses JSON manifests in a centralized `ManifestPlugins/` directory, merged into a host-level manifest. The LMS approach decouples module manifests from module source trees (manifests can live outside the module project). This is a valid design — retain and enhance rather than replacing with `Module.txt`.

**IY.Modules recommendation:** Keep JSON manifests. Add explicit fields for `Id`, `Version`, `Dependencies[]`, `Category`, `IsRequired`.

---

### Feature Dependencies — Adopt

Orchard Core's topological sort over declared module dependencies is critical for deterministic initialization. LMS has no equivalent. Without this, a module that requires another module's services may silently fail at request time.

**IY.Modules recommendation:** Manifest JSON should declare `"Dependencies": ["ModuleA", "ModuleB"]`. Loader performs topological sort before loading begins.

---

### Application Composition / IStartup — Already Covered

Orchard Core's `IStartup` interface (with `ConfigureServices` + `Configure`) maps directly to LMS's `IModule.RegisterServices` + the host's `ConfigureApplication`. LMS already achieves this split — but only at the host level; individual modules cannot contribute middleware.

**IY.Modules recommendation:** Extend `IModule` to include:
```csharp
void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider services);
```

---

### Lifecycle Hooks — Adopt

Orchard Core's `IModularTenantEvents` provides async activation/deactivation hooks per tenant. LMS has no equivalent. This is a gap that IY.Modules must fill.

**IY.Modules recommendation:**
```csharp
interface IModuleLifecycle
{
    Task OnStartingAsync(IServiceProvider services, CancellationToken ct);
    Task OnStartedAsync(IServiceProvider services, CancellationToken ct);
    Task OnStoppingAsync(CancellationToken ct);
}
```

---

### Tenant Isolation — Defer

Orchard Core's multi-tenancy is a significant architectural investment with per-tenant DI containers, shell contexts, and settings. LMS is single-tenant. IY.Modules should not attempt multi-tenancy in the initial version.

**Recommendation:** Defer. Design the `IModule` contract to be tenant-neutral (no `ITenantContext` coupling) so tenant support can be added later without breaking the interface.

---

### Runtime Activation — Adopt Partially

Orchard Core allows enabling/disabling features at runtime, but this requires rebuilding the shell (per-tenant restart). It does not support true zero-downtime hot-swap of assemblies. LMS has no equivalent at all.

**IY.Modules recommendation (initial):** Support runtime enable/disable with a graceful restart. Do not attempt zero-downtime assembly hot-swap in v1.

---

### Assembly Loading — Adopt Conceptually (ALC)

Orchard Core's `IExtensionLoader` implementations use isolated `AssemblyLoadContext` per extension type. This prevents version conflicts between modules. LMS uses the default ALC.

**IY.Modules recommendation:** Introduce per-module `AssemblyLoadContext` (non-collectible for v1 to maintain LMS compatibility; collectible in v2 for true isolation and unloading).

---

### Middleware Contribution — Adopt

Orchard Core modules contribute middleware via `IStartup.Configure`. LMS has no equivalent — the host owns the entire middleware pipeline. This prevents modules from adding response compression, request transformation, or custom authentication schemes without host modification.

**IY.Modules recommendation:** Allow modules to register middleware through a controlled pipeline builder interface.

---

### Diagnostics — Adopt ILogger

Orchard Core uses `ILogger<T>` throughout. LMS uses `Console.WriteLine`. When running under Docker or Application Insights, `Console.WriteLine` is invisible in structured logs.

**IY.Modules recommendation:** `ILogger<ModuleManager>` (or equivalent) for all module lifecycle events.

---

## Not Applicable From Orchard Core

The following Orchard Core concepts are not applicable to IY.Modules/LMS:
- CMS-specific concepts (content types, workflows, media management)
- Liquid template engine
- Tenant shell storage (`IShellConfiguration`)
- Theme system (`IThemeManager`)
- Recipe/setup step system

These are domain-specific to CMS and would be over-engineered for LMS.
