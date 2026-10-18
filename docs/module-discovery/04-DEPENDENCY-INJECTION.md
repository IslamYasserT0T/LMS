# LMS-D04 — Dependency Injection and Service Composition

**Workstream:** LMS-D04  
**Status:** Confirmed implementation — fully traced.

---

## 1. Registration Timeline

```
Phase 1: Bootstrapper.PreInitialize()
  ├── AddDbContext<BaseDbContext>           (host-global, PostgreSQL or SQLite)
  ├── AddAuthentication()
  ├── AddSettingModule(configuration)       (SettingModule static registration)
  ├── AddAuthorizationModule(configuration) (AuthorizationModule static registration)
  ├── AddControllers()
  ├── AddBlazorise / AddBootstrap5 / AddFontAwesome
  ├── AddRazorComponents().AddInteractiveServerComponents()
  ├── AddCascadingAuthenticationState()
  ├── AddServerSideBlazor()
  ├── AddMudServices()
  ├── AddLocalization()
  ├── AddSingleton<IModuleManager>(moduleManager)
  ├── AddEndpointsApiExplorer()
  └── AddSwaggerGen(...)

Phase 2: Bootstrapper.Initialize() → moduleManager.LoadModules(services, config, env)
  └── foreach plugin in plugins/:
        Assembly.LoadFrom(dll)
        IModule.RegisterServices(services, configuration, hostEnvironment)
        // Each module adds its own services here

Phase 3: Bootstrapper.PostInitialize()
  ├── AddRazorPages() + AddApplicationPart(uiAssembly) foreach UI module
  ├── AddModuleAutoMappers(allAssemblies)   (scans IHasAutoMapperProfiles)
  └── AddModuleValidators(allAssemblies)    (FluentValidation scanning)
```

---

## 2. Module-Specific Registrations

### SampleModule.API — Module.RegisterServices
**File:** `modules/SampleModule/SampleModule.API/Module.cs` L31-34

```csharp
public void RegisterServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllers().AddApplicationPart(typeof(SampleController).Assembly);
}
```
Note: `AddControllers()` is called again inside the module. Duplicate calls to `AddControllers()` are safe in ASP.NET Core (idempotent), but each `AddApplicationPart` adds a new part.

### SampleModule.Infrastructure — AddInfrastructureSampleModuleServices
**File:** `modules/SampleModule/SampleModule.Infrastructure/Extensions/SampleModuleServiceCollections.cs`

Registrations:
- `AddDbContext<SampleModuleDBContext>` (own context, PostgreSQL or SQLite)
- `AddScoped<IBaseDbContext, SampleModuleDBContext>`
- `AddScoped<ISampleRepository, SampleRepository>`
- `AddScoped<IModuleDbMigrator, SampleModuleMigrator>`

### SampleModule.Infrastructure — AddRepositoryServices (ServiceRepository)
**File:** `modules/SampleModule/SampleModule.Infrastructure/Extensions/ServiceRepository.cs`

Assembly scanning pattern:
- Scans `Assembly.GetExecutingAssembly().ExportedTypes`
- Finds types implementing `IApplicationService` (and not interface/abstract)
- Registers by convention: `IFoo → Foo` based on `"I" + serviceType.Name`
- Lifetime determined by `IScopedService`, `ITransientService`, `ISingletonService`

### LMS.IdentityModule — AddLMSIdentityModule
**File:** `modules/IdentityModule/LMS.IdentityModule/IdentityModuleServiceCollectionExtensions.cs`

Key registrations:
- `services.AddIYAuth().UseEntityFrameworkCore(...).UseOpenIddict(...)`
- `AddScoped<IModuleDbMigrator, IdentityModuleDbMigrator>`
- `AddScoped<IAuthThemeProvider, LmsAuthThemeProvider>`
- `AddLocalization()`
- `AddSingleton<Action<IEndpointRouteBuilder>>(endpoints => endpoints.MapIYAuthEndpoints())`

The last registration is a notable pattern: the module cannot register endpoints during DI setup, so it registers a delegate that the host invokes in `ConfigureApplication` (Bootstrapper.cs L184-187).

### LMS.ReportingModule — Module.RegisterServices
**File:** `modules/ReportingModule/LMS.ReportingModule/Module.cs`

Key registrations:
- `AddLocalization()`
- `AddReporting()` — IY.Reporting core
- `AddSqlKataReporting(factory)` — QueryFactory via SqlKata
- `AddScoped<IReportExecutionContext, LmsReportExecutionContext>`
- `AddScoped<IReportAccessPolicy, LmsReportAccessPolicy>`
- `AddScoped<IReportDataScopeProvider, LmsReportDataScopeProvider>`
- `AddScoped<IReportExportPolicy, LmsReportExportPolicy>`
- `AddSingleton(LmsReportingSchema.Create())`
- `AddReportingBlazor()`

---

## 3. Service Provider Creation

`builder.Build()` is called in `Program.Main` (line 25) **after** all `PreInitialize`, `Initialize`, and `PostInitialize` phases complete. This means the service provider is built once with all module services included.

**Implication:** No module can register services after `builder.Build()` returns. Any post-startup service registration would require a new provider or a mutable DI abstraction — neither of which LMS implements.

---

## 4. Service Lifetimes Used by Modules

| Service | Lifetime | Module |
|---|---|---|
| `SampleModuleDBContext` | Scoped | SampleModule.Infrastructure |
| `IBaseDbContext` | Scoped | SampleModule.Infrastructure |
| `ISampleRepository` | Scoped | SampleModule.Infrastructure |
| `IModuleDbMigrator` | Scoped | All modules |
| `IReportExecutionContext` | Scoped | ReportingModule |
| `IReportAccessPolicy` | Scoped | ReportingModule |
| `IReportDataScopeProvider` | Scoped | ReportingModule |
| `IReportExportPolicy` | Scoped | ReportingModule |
| `LmsReportingSchema` | Singleton | ReportingModule |
| `Action<IEndpointRouteBuilder>` | Singleton | IdentityModule |
| `IAuthThemeProvider` | Scoped | IdentityModule |
| `IModuleManager` | Singleton | LMS.Host |

---

## 5. Host-Global vs. Module-Local Services

LMS uses a **single shared service container**. There are no per-module DI containers or scopes. All module services are registered into the same `IServiceCollection` that the host uses.

Consequences:
- Module A can resolve services registered by Module B (no isolation).
- Service name collisions are possible (no namespace enforcement).
- Captive dependency risk: a singleton can receive a scoped dependency.

---

## 6. Open Generic Registrations

None found in the module or shared codebase. Generic registrations are not used.

---

## 7. Registration Ordering

1. Host infrastructure (Phase 1)
2. SettingModule and AuthorizationModule (Phase 1, lines 74-75)
3. Module plugins in filesystem traversal order (Phase 2)
4. AutoMapper, Validators, Razor pages (Phase 3)

There is no guarantee of ordering between plugin modules. If Module B depends on a service registered by Module A, and Module A's directory sorts after Module B's, the dependency may not be resolvable during Module B's `RegisterServices`. However, since all registrations complete before `builder.Build()`, the actual service provider construction handles it correctly. The risk is in per-module wiring that attempts to resolve services at registration time (e.g., `services.AddSingleton<IFoo>(provider => provider.GetRequiredService<IBar>())` would fail during factory execution if order matters, but no such pattern was found in the inspected code).

---

## 8. Anti-Patterns Found

| Anti-pattern | Location | Risk |
|---|---|---|
| Duplicate `AddControllers()` call | `SampleModule.API/Module.cs` L33 | Minor, safe in ASP.NET Core |
| `ModuleManager` stores `IServiceCollection` as private field | `ModuleManager.cs` L20 | Lifetime leakage risk — `IServiceCollection` should not be held after startup |
| `ModuleManager` hard-coded to `SettingDbContext` | `ModuleManager.cs` L84 | Tight coupling to SettingModule; breaks if SettingModule removed |
| Module DI registration may fail silently | `ModuleManager.cs` L159-160 | Exceptions caught, logged to console, module skipped |

---

## 9. Can a Module Register Services After Host Start?

**No.** `IServiceCollection` is immutable after `builder.Build()` is called. The host provides no mechanism for post-startup service registration. The `IModuleManager.LoadModules` must be called before `builder.Build()`.
