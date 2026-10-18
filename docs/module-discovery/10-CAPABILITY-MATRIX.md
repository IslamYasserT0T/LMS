# LMS-D10 — Runtime Management Capability Matrix

**Workstream:** LMS-D10  
**Status:** Confirmed per source inspection.

---

## Capability Matrix

| Capability | Classification | Evidence | Notes |
|---|---|---|---|
| Module discovery | **Implemented and verified** | `ModuleManager.LoadModule()` L123-170 | Directory scan + IModule reflection |
| Module registration | **Implemented and verified** | `ModuleManager.RegisterModuleServices()` L232-244 | `IModule.RegisterServices()` called per plugin |
| Dependency resolution | **Partially implemented** | ASP.NET Core DI container | No inter-module dependency declaration; host DI resolves service dependencies only |
| Dependency cycle detection | **Not implemented** | No topological sort found | ASP.NET Core DI does not detect module-level cycles |
| Deterministic initialization | **Not implemented** | `Directory.GetDirectories` L129 | Filesystem-order iteration; non-deterministic |
| Runtime activation | **Not implemented** | No runtime management API found | Modules are loaded only at startup |
| Runtime deactivation | **Not implemented** | No runtime management API found | Cannot disable a loaded module without restart |
| Runtime reload | **Not implemented** | No reload mechanism found | Process restart required |
| Module isolation | **Not implemented** | `Assembly.LoadFrom` L137 | Default ALC; no per-module isolation |
| Assembly unloading | **Not implemented** | Default ALC is non-collectible | Cannot unload without process restart |
| Module health | **Not implemented** | No health checks registered | No `IHealthCheck` per module |
| Failure recovery | **Not implemented** | Catch+`Console.WriteLine` L155-161 | Silent failure; no quarantine or retry |
| Lifecycle diagnostics | **Not implemented** | No diagnostics/events/metrics | No `ILogger`, `IMetrics`, or `ActivitySource` per module lifecycle |

---

## Detailed Classification Explanations

### Module Discovery — Implemented and Verified
Discovery is fully implemented via the manifest pipeline and directory scan. Files: `ModuleManifestsManager.cs`, `ModuleFolderCopier.cs`, `ModuleManager.LoadModule()`. Triggered at application startup, once. No runtime re-discovery.

### Module Registration — Implemented and Verified
`ModuleManager.RegisterModuleServices` instantiates the `IModule` via `Activator.CreateInstance` and calls `RegisterServices`. All services are registered into the single shared `IServiceCollection`. Verified in SampleModule.API, IdentityModule, ReportingModule.

### Dependency Resolution — Partially Implemented
The ASP.NET Core DI container resolves service-level dependencies (e.g., `ISampleRepository` → `SampleRepository`). However, there is no mechanism for declaring that "Module A requires Module B to be loaded first." Inter-module service dependencies work only if both modules are loaded (DI resolution at request time). No ordered dependency graph.

### Dependency Cycle Detection — Not Implemented
ASP.NET Core's built-in DI container validates captive dependencies (scoped inside singleton) but does not detect module-level registration cycles. If Module A's services depend on Module B and vice versa (at module metadata level), there is no detection mechanism.

### Deterministic Initialization — Not Implemented
`Directory.GetDirectories` (L129) does not guarantee ordering. On Windows NTFS it is generally creation order, but this is undocumented and unreliable. No explicit sort or dependency-ordered loading is implemented.

### Runtime Activation — Not Implemented
There is no API, endpoint, or background service that enables a new module after `app.Run()` has been called. The service provider is immutable after `builder.Build()`, so runtime activation cannot register new services.

### Runtime Deactivation — Not Implemented
No mechanism to disable or unload a module without restarting the process. The module's services remain registered in the DI container for the lifetime of the process.

### Runtime Reload — Not Implemented
No hot-reload, assembly swap, or graceful restart with module changes. A new deployment requires a full process restart.

### Module Isolation — Not Implemented
All modules run in the same `AssemblyLoadContext` (default), same process, same DI container. A type exception or memory corruption in one module can affect all others.

### Assembly Unloading — Not Implemented
The default ALC is non-collectible. `Assembly.LoadFrom` assemblies cannot be unloaded. This is a .NET runtime constraint.

### Module Health — Not Implemented
No `IHealthCheck` implementations found for module infrastructure. ASP.NET Core health endpoints (`/health`) are not configured. No per-module health signal after startup.

### Failure Recovery — Not Implemented
Exception handling during module load: `catch (Exception ex) { Console.WriteLine(...); }`. The application continues without the failed module. No:
- Retry
- Quarantine state
- Health degraded signal
- Alert
- Automatic restart

### Lifecycle Diagnostics — Not Implemented
No `ILogger<ModuleManager>` used. No `ActivitySource` tracing. No `IMetrics` recording. All diagnostics are `Console.WriteLine` calls only, which are invisible in structured logging systems.

---

## Summary

LMS implements the minimum viable module infrastructure: discover, load once, register. Every advanced capability required for a production module management system (runtime lifecycle, isolation, health, diagnostics, ordering) is absent and must be designed and built in IY.Modules.
