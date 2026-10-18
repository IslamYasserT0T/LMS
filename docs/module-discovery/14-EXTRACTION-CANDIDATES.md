# LMS-D14 — Extraction Candidates

**Workstream:** LMS-D14  
**Classification:** Extract as-is / Adapt / Redesign / Reject

---

## Group A — Extract As-Is (Minimal Adaptation)

These components are clean, non-LMS-specific, and can be moved into an `IY.Modules` NuGet package with namespace changes only.

### A1. `IModule` Interface
**File:** `shared/Shared.Core/Contracts/IModule.cs`  
**Why:** Pure abstraction with no LMS-specific dependencies. `RegisterServices` signature uses only `Microsoft.Extensions.*` types.  
**Change needed:** Add lifecycle methods (`Initialize`, `Configure`, `Stop`). Rename namespace.  
**Target:** `IY.Modules.Abstractions.IModule`

### A2. `IUiModule` Interface
**File:** `shared/Shared.Core/Contracts/IUiModule.cs`  
**Why:** Empty marker. Blazor-specific but not LMS-specific.  
**Change needed:** Namespace only.  
**Target:** `IY.Modules.Abstractions.IUiModule`

### A3. `ISingletonService` / `IScopedService` / `ITransientService` / `IApplicationService`
**Files:** `shared/Shared.Core/DependencyInjections/`  
**Why:** Generic DI lifetime markers with no dependencies.  
**Change needed:** Namespace only.  
**Target:** `IY.Modules.Abstractions.DependencyInjections.*`

### A4. `IModuleDbMigrator`
**File:** `shared/Shared.Infrastructure/Persistence/IModuleDbMigrator.cs`  
**Why:** Clean two-method interface with only `IServiceProvider` dependency.  
**Change needed:** Add async overloads. Namespace.  
**Target:** `IY.Modules.Abstractions.Persistence.IModuleDbMigrator`

### A5. `ModuleFolderCopier` (Logic)
**File:** `src/LMS.Host/Services/ModuleFolderCopier.cs`  
**Why:** File-copy utility with path traversal protection and Release/Debug guard logic. Pure filesystem operations.  
**Change needed:** Decouple from `LMS.Host.Dtos` types. Make manifest DTO types shareable.  
**Target:** `IY.Modules.Deployment.ModuleFolderCopier`

### A6. `ModuleManifestsManager` (Logic)
**File:** `src/LMS.Host/Services/ModuleManifestsManager.cs`  
**Why:** JSON manifest merging and token substitution. No LMS-specific logic.  
**Change needed:** None beyond namespace and DTO.  
**Target:** `IY.Modules.Deployment.ModuleManifestsManager`

### A7. Manifest DTO types
**File:** `src/LMS.Host/Dtos/` (ModulesRoot, ModulesManifest, ModuleDefinition)  
**Why:** Plain JSON-serializable POCOs.  
**Change needed:** Namespace only.  
**Target:** `IY.Modules.Deployment.Manifests.*`

### A8. `ServiceRepository` (Assembly-Scan Pattern)
**File:** `modules/SampleModule/SampleModule.Infrastructure/Extensions/ServiceRepository.cs`  
**Why:** Convention-driven DI registration by lifetime marker interfaces. Reusable generic utility.  
**Change needed:** Generalize to any assembly, not just `Assembly.GetExecutingAssembly()`.  
**Target:** `IY.Modules.ServiceCollectionExtensions.AddModuleServices(assembly)`

---

## Group B — Adapt (Structural Changes Required)

These components are conceptually correct but tightly coupled to LMS-specific types or patterns.

### B1. `ModuleManager` — Core Loading Logic
**File:** `src/LMS.Host/Services/ModuleManager.cs`  
**Why extractable concept:** Plugin directory scan, `Assembly.LoadFrom`, `IModule` type scan, companion Blazor assembly discovery.  
**LMS coupling:** Hard dependency on `SettingDbContext` (L84), returns `LMS.Host.Services.ModuleInfo`, uses `NavMenuItem` (Blazor concern).  
**Adaptation:**
- Extract generic `IModuleLoader` that discovers and loads plugins.
- Remove `SettingDbContext` dependency → replace with `IEnumerable<IModuleStartupHook>`.
- Separate Blazor UI module tracking into a Blazor-specific extension.

### B2. `IModuleManager` Interface
**File:** `src/LMS.Host/Contracts/IModuleManager.cs`  
**Why extractable concept:** Contract for module lifecycle orchestration.  
**LMS coupling:** Returns `List<NavMenuItem>` (Blazor UI concern), returns `LMS.Host.Services.ModuleInfo`.  
**Adaptation:** Generic `IModuleHost` contract with separate UI extension point.

### B3. `ModuleInfo`
**File:** `src/LMS.Host/Services/ModuleInfo.cs`  
**Why extractable:** Runtime descriptor holding `IModule` + `Assembly`.  
**LMS coupling:** Namespace only.  
**Adaptation:** Enrich with state, version, dependency declarations. Namespace change.

### B4. `Bootstrapper` (Startup Orchestration Pattern)
**File:** `src/LMS.Host/Services/Bootstrapper.cs`  
**Why extractable concept:** Four-phase startup (`PreInitialize → Initialize → PostInitialize → ConfigureApplication`).  
**LMS coupling:** Direct Blazorise, MudBlazor, Casbin, SettingModule references inside phases. Startup logic and infrastructure wiring are interleaved.  
**Adaptation:** Separate the phase contract (interface) from the LMS-specific wiring. IY.Modules provides `IModuleHost.PreInitialize/Initialize/PostInitialize/Configure` as extension points; host calls them without knowing the internals.

### B5. `ISettingDefinationProvider` (as `IModuleStartupHook`)
**File:** `shared/Modules/SettingModule/Contracts/ISettingDefinationProvider.cs`  
**Why extractable concept:** Post-startup reflection-discovered initialization hook.  
**LMS coupling:** Returns `ISettingDefinationContext` (SettingModule-specific).  
**Adaptation:** Generalize as `IModuleInitializer.InitializeAsync(IServiceProvider, CancellationToken)`. SettingModule-specific behavior becomes an LMS-specific implementation of this generic interface.

---

## Group C — Redesign (Too Coupled or Missing Critical Design)

### C1. Assembly Loading Strategy
**Current:** `Assembly.LoadFrom` into default ALC.  
**Problem:** No isolation, no unloading capability.  
**Redesign:** Per-module `AssemblyLoadContext`. v1: non-collectible (LMS-compatible). v2: collectible (full isolation + unloading).

### C2. Module Ordering / Dependency Resolution
**Current:** Not implemented. Filesystem order.  
**Problem:** Non-deterministic. No inter-module dependency awareness.  
**Redesign:** Topological sort over manifest-declared `Dependencies[]`. Detect cycles. Fail-fast.

### C3. Failure Handling
**Current:** `catch + Console.WriteLine` — silent continuation.  
**Problem:** Application can run in partially-initialized state with no observable signal.  
**Redesign:** Per-module failure policy (`Fatal`, `Quarantine`, `Warn`). Degraded health check per failed module. Structured logging.

### C4. Lifecycle Contract
**Current:** `IModule.RegisterServices` only (single phase, synchronous).  
**Problem:** No `Initialize`, `Start`, `Stop`, `Dispose` hooks. No `IHostedService` per module.  
**Redesign:** Multi-phase `IModule` or separate `IModuleLifecycle`, `IModuleHostedService`.

### C5. Runtime Management
**Current:** None.  
**Problem:** Cannot activate/deactivate without restart.  
**Redesign:** `IModuleRuntime` with enable/disable state machine. Restart-required policy for v1. True dynamic loading in v2.

---

## Group D — Reject (LMS-Specific, Not Relevant to IY.Modules Core)

| Component | Reason |
|---|---|
| `INavigationProvider` / `NavMenuItem` | Blazor navigation UI — not part of module framework |
| `LmsAuthThemeProvider` | LMS branding — not part of module framework |
| `App.razor` / Blazor host components | Host UI — not part of module framework |
| `PermissionsController.cs` | LMS permissions API — not part of module framework |
| Casbin `IEnforcer` configuration | Authorization strategy — separate concern |

---

## Extraction Priority Order for IY.Modules v1

1. `IY.Modules.Abstractions` NuGet: `IModule`, `IUiModule`, `IModuleDbMigrator`, DI markers
2. `IY.Modules.Deployment` NuGet: `ModuleFolderCopier`, `ModuleManifestsManager`, manifest DTOs
3. `IY.Modules.Hosting` NuGet: `ModuleManager` (redesigned), `IModuleHost`, module lifecycle
4. `IY.Modules.AspNetCore` NuGet: ASP.NET Core integration (middleware pipeline, Blazor assemblies, endpoint registration)
5. Tests: Unit tests for all Group A and B components with full lifecycle coverage
