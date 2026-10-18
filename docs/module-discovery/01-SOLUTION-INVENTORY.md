# LMS-D01 — Solution and Project Inventory

**Source:** `d:\LMS\LMS.sln`  
**Target Framework:** net9.0 (all projects)  
**SDK:** Microsoft.NET.Sdk / Microsoft.NET.Sdk.Web  
**Visual Studio Version:** 17.0.31903.59

---

## 1. Host Projects

| Project | Path | Role |
|---|---|---|
| `LMS.Host` | `src/LMS.Host/` | ASP.NET Core web host, Bootstrapper, ModuleManager |

### LMS.Host key files
- `Program.cs` — Entry point, orchestrates `Bootstrapper`
- `Services/Bootstrapper.cs` — Four-phase startup orchestrator
- `Services/ModuleManager.cs` — Plugin discovery, assembly loading, service registration
- `Services/ModuleManifestsManager.cs` — Manifest merge and Configuration substitution
- `Services/ModuleFolderCopier.cs` — File-system deployment copier
- `Services/ModuleInfo.cs` — Runtime module descriptor
- `Contracts/IModuleManager.cs` — Host contract for module management
- `manifest` (no extension) — Merged runtime manifest (JSON)

---

## 2. Shared Libraries

| Project | Path | Role |
|---|---|---|
| `Shared.Core` | `shared/Shared.Core/` | Core contracts: `IModule`, `IUiModule`, `IData`, DI marker interfaces |
| `Shared.Domain` | `shared/Shared.Domain/` | Domain base types |
| `Shared.Application` | `shared/Shared.Application/` | Application-layer base types, AutoMapper profile contracts |
| `Shared.Infrastructure` | `shared/Shared.Infrastructure/` | `BaseDbContext`, `IModuleDbMigrator`, repository base types |
| `Shared.Ui` | `shared/Shared.Ui/` | Shared Blazor UI components |

### Shared.Core important types
- `Shared.Core.Contracts.IModule` — The plugin entry-point interface
- `Shared.Core.Contracts.IUiModule` — Marker for Blazor-UI plugin
- `Shared.Core.Contracts.IData` — Marker (empty)
- `Shared.Core.DependencyInjections.ISingletonService` — Lifetime marker
- `Shared.Core.DependencyInjections.IScopedService` — Lifetime marker
- `Shared.Core.DependencyInjections.ITransientService` — Lifetime marker
- `Shared.Core.DependencyInjections.IApplicationService` — Base marker for scannable services

**Notable coupling:** `Shared.Core.csproj` carries direct `ProjectReference` to `SettingModule` and `AuthorizationModule` (lines 17-18). This means any consumer of `Shared.Core` transitively depends on those shared modules.

---

## 3. Shared (Built-in) Module Projects

| Project | Path | Role |
|---|---|---|
| `SettingModule` | `shared/Modules/SettingModule/` | Setting persistence, `ISettingDefinationProvider` lifecycle hook |
| `AuthorizationModule` | `shared/Modules/AuthorizationModule/` | Casbin-based RBAC, `IEnforcer`, EF Core adapter |

These modules are NOT loaded as plugins — they are statically referenced and registered in `Bootstrapper.PreInitialize` at lines 74-75.

---

## 4. Plugin Module Projects

### SampleModule
Multi-assembly plugin: `SampleModule.API`, `SampleModule.Application`, `SampleModule.Core`, `SampleModule.Domain`, `SampleModule.Infrastructure`, `SampleModule.Ui`

| Sub-project | Role |
|---|---|
| `SampleModule.API` | `Module.cs` (IModule impl), controllers |
| `SampleModule.Application` | Services, contracts, DTOs, validators, AutoMapper profiles |
| `SampleModule.Core` | Core value objects |
| `SampleModule.Domain` | Entities, domain interfaces, repository contracts |
| `SampleModule.Infrastructure` | EF Core DbContext, repositories, migrations, `SampleModuleMigrator` |
| `SampleModule.Ui` | Blazor razor components, pages |

### LMS.IdentityModule
Single-assembly plugin backed by `IY.Auth`.

| File | Role |
|---|---|
| `Module.cs` | `IModule` impl, delegates to `AddLMSIdentityModule` |
| `IdentityModuleServiceCollectionExtensions.cs` | Full DI registration, IY.Auth EF Core, OpenIddict, theme provider |
| `IdentityModuleDbMigrator.cs` | `IModuleDbMigrator` impl via `IIdentityDatabaseMigrator` |
| `LmsAuthThemeProvider.cs` | `IAuthThemeProvider` impl for Blazor components |

### LMS.ReportingModule
Single-assembly plugin backed by `IY.Reporting`.

| File | Role |
|---|---|
| `Module.cs` | `IModule` impl, full service registration with Debug/Release gating |
| Various `Lms*.cs` | Production adapters: execution context, access policy, data scope, export policy |
| `Proof*.cs` | Debug-only synthetic test harness types |
| `Ui/` | Blazor pages: `ReportsIndex`, `ReportsViewerPage`, `ReportsDesignerPage` |

---

## 5. Test Projects

| Project | Path | Coverage |
|---|---|---|
| `SampleModule.Test` | `modules/SampleModule/SampleModule.Test/` | Service-layer tests |
| `LMS.ReportingModule.Test` | `modules/ReportingModule/LMS.ReportingModule.Test/` | `ModuleFolderCopier`, `ModuleManifestsManager`, security isolation, component verification |

---

## 6. Manifest Plugin Definitions

| File | Covers |
|---|---|
| `ManifestPlugins/SampleModuleMinifest.json` | SampleModule 6 sub-assemblies |
| `ManifestPlugins/IdentityModuleManifest.json` | LMS.IdentityModule 1 assembly |
| `ManifestPlugins/ReportingModuleManifest.json` | LMS.ReportingModule 1 assembly |

---

## 7. Project Dependency Map

```
LMS.Host
  ├── Shared.Application
  ├── Shared.Core
  │   ├── SettingModule          (statically referenced)
  │   └── AuthorizationModule    (statically referenced)
  ├── Shared.Domain
  ├── Shared.Infrastructure
  │   └── Shared.Domain
  └── Shared.Ui

[Plugin assemblies — loaded at runtime via Assembly.LoadFrom]
  ├── SampleModule.API
  │   ├── SampleModule.Application
  │   ├── SampleModule.Infrastructure
  │   └── SampleModule.Core
  ├── LMS.IdentityModule
  │   └── IY.Auth.* (NuGet)
  └── LMS.ReportingModule
      └── IY.Reporting.* (NuGet)
```

---

## 8. Major NuGet Dependencies

| Package | Version | Consumer |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 9.0.7 | Shared.Infrastructure, multiple modules |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | Shared.Infrastructure, modules |
| `Microsoft.EntityFrameworkCore.Sqlite` | 9.0.7 | LMS.Host (test) |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 9.0.7 | Shared.Core |
| `Microsoft.Extensions.Hosting.Abstractions` | 9.0.0 | Shared.Core |
| `Swashbuckle.AspNetCore` | 9.0.3 | Shared.Core |
| `AutoMapper` | (via Shared.Application) | LMS.Host |
| `Blazorise` / `MudBlazor` | (Blazor UI) | LMS.Host |
| `IY.Auth.*` | (private NuGet) | LMS.IdentityModule |
| `IY.Reporting.*` | 1.0.1 | LMS.ReportingModule |
| `Casbin.NET` + `Casbin.Persist.Adapter.EFCore` | (RBAC) | AuthorizationModule |
| `SqlKata` | (query builder) | LMS.ReportingModule |
| `OpenIddict` | (via IY.Auth) | LMS.IdentityModule |
| `xUnit` | (testing) | Test projects |
