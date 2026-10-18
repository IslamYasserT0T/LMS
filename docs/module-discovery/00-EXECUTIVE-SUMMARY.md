# LMS Module Architecture — Executive Summary

**Discovery phase:** Phase A  
**Discovery date:** 2026-10-13  
**Workspace:** `d:\LMS`  
**Target:** LMS.sln (net9.0, Visual Studio 17)  
**Mission:** Inform IY.Modules framework design — discovery only, no production changes made.

---

## Discovery Verdict: COMPLETE

All 12 workstreams were executed through source-code inspection. No production files were modified, no migrations were run, no destructive operations were performed.

---

## Architecture Summary

LMS uses a **manifest-driven, file-system plugin loader** that runs at application startup. Modules are compiled separately, published to a `plugins/` folder structure, and then discovered and loaded via `Assembly.LoadFrom`. Each plugin exposes exactly one class that implements `IModule` (`Shared.Core.Contracts.IModule`). The host's `Bootstrapper` class drives a four-phase startup sequence: `PreInitialize → Initialize → PostInitialize → ConfigureApplication`. There is no runtime activation, deactivation, or unloading.

---

## Major Findings

### Strengths

| Finding | Evidence |
|---|---|
| Clear `IModule` contract separates plugin from host | `shared/Shared.Core/Contracts/IModule.cs` |
| Manifest-driven deployment decouples build from host config | `ManifestPlugins/*.json`, `src/LMS.Host/manifest` |
| MSBuild `.targets` automate plugin publishing | `modules/*/PluginManifest.targets` |
| `IModuleDbMigrator` provides a migration boundary per module | `shared/Shared.Infrastructure/Persistence/IModuleDbMigrator.cs` |
| Satellite assembly resolver handles i18n resource DLLs | `ModuleManager.cs` L247-286 |
| `ISettingDefinationProvider` enables post-build setting registration | `shared/Modules/SettingModule/Contracts/ISettingDefinationProvider.cs` |
| `Action<IEndpointRouteBuilder>` pattern allows modules to register endpoints without a host dependency | `LMS.IdentityModule/IdentityModuleServiceCollectionExtensions.cs` L47 |
| Companion Blazor assembly discovery for UI plugins | `ModuleManager.cs` L178-216 |
| Tests cover copier security, manifest resolution, and Debug/Release isolation | `LMS.ReportingModule.Test/` |

### Confirmed Limitations

| Limitation | Evidence |
|---|---|
| `Assembly.LoadFrom` into default ALC — no isolation | `ModuleManager.cs` L137 |
| No `AssemblyLoadContext` or `AssemblyDependencyResolver` | Entire codebase grep |
| Modules cannot be activated or deactivated at runtime | No runtime state machine found |
| No lifecycle hooks beyond `RegisterServices` | `IModule.cs` — single method |
| Module startup ordering is filesystem-iteration order — non-deterministic | `ModuleManager.cs` L129 |
| No dependency declaration between modules | No dependency graph found |
| No cycle detection | No topological sort found |
| `Shared.Core` project has a build-time reference to `SettingModule` and `AuthorizationModule` | `Shared.Core.csproj` L17-18 |
| `ModuleManager` is hard-wired to `SettingDbContext` | `ModuleManager.cs` L84 |
| No module health monitoring or failure recovery | No health checks found |
| Module startup errors are swallowed (`Console.WriteLine`) — application continues | `ModuleManager.cs` L159-160 |

---

## Extraction Recommendation

### Can be reused with minimal changes
- `IModule`, `IUiModule`, `IData` contracts
- `IModuleDbMigrator` interface
- `ISettingDefinationProvider` / `ISettingDefinationContext` pattern
- `ModuleFolderCopier` (file-copy logic)
- `ModuleManifestsManager` (manifest merge logic)
- Manifest JSON schema (`ModulesManifest`, `ModulesRoot`, `ModuleDefinition`)
- `ISingletonService` / `IScopedService` / `ITransientService` / `IApplicationService` marker interfaces
- `ServiceRepository` assembly-scanning pattern

### Must be redesigned for IY.Modules
- Module lifecycle (add `Initialize`, `Start`, `Stop`, `Dispose` hooks)
- `Assembly.LoadFrom` → `AssemblyLoadContext` per module
- Dependency ordering (topological sort on declared dependencies)
- Error handling (failed module must quarantine, not silence)
- `ModuleManager`'s hard coupling to `SettingDbContext`
- Runtime activation / deactivation capability
- Module health and diagnostics

---

## Implementation Prerequisites (Decisions Required Before Building IY.Modules)

1. **Assembly isolation level:** Default ALC (shared, LMS-compatible) vs. per-module isolated ALC (collectible, breaking).
2. **Module dependency declaration:** Convention (naming), attribute, or manifest-based.
3. **Lifecycle contract:** How many phases? Are phases async? Can phases be skipped?
4. **Runtime management surface:** Admin HTTP API, background service, or both?
5. **Migration ownership:** Does IY.Modules own migration orchestration or defer to host?
6. **Failure policy:** Fatal (stop host), quarantine (disable module), or ignore (current behavior)?
7. **Setting integration:** Should IY.Modules remain coupled to `ISettingDefinationProvider` or make it an optional extension point?

---

## Handoff

Full source-referenced, self-contained handoff document:
[`15-IY-MODULES-HANDOFF.md`](./15-IY-MODULES-HANDOFF.md)
