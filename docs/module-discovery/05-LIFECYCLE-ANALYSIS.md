# LMS-D05 — Lifecycle and Startup Orchestration

**Workstream:** LMS-D05  
**Status:** Confirmed implementation — fully traced.

---

## 1. Full Startup Sequence

**Source:** `src/LMS.Host/Program.cs` + `src/LMS.Host/Services/Bootstrapper.cs`

```
[1] Program.Main(args)
    │
    ├─[2] WebApplication.CreateBuilder(args)   → IWebApplicationBuilder
    │
    ├─[3] new Bootstrapper(builder)
    │       ModuleManager = new ModuleManager()
    │
    ├─[4] await bootstrapper.PreInitialize()
    │       ├── Walk directories to find LMS.sln (solution-root detection)
    │       ├── ModuleManifestsManager.ProcessManifests(manifestSource, manifestDest, config)
    │       │     Merge ManifestPlugins/*.json → src/LMS.Host/manifest
    │       ├── ModuleFolderCopier.CopyFoldersFromManifestAsync(manifest, plugins/, config)
    │       │     Copy each module's published output into plugins/<Name>/
    │       ├── AddDbContext<BaseDbContext>(...)
    │       ├── AddAuthentication()
    │       ├── AddSettingModule(configuration)
    │       ├── AddAuthorizationModule(configuration)
    │       ├── AddControllers()
    │       ├── AddBlazorise / MudBlazor / Razor / Blazor Server
    │       ├── AddSingleton<IModuleManager>(moduleManager)
    │       └── AddSwaggerGen(...)
    │
    ├─[5] await bootstrapper.Initialize(moduleManager)
    │       └── moduleManager.LoadModules(services, config, env)
    │             ├── EnsureSatelliteAssemblyResolverRegistered()
    │             └── LoadModule()
    │                   foreach dir in plugins/
    │                     Assembly.LoadFrom(dir/Name.dll)  ← no isolation
    │                     IModule.RegisterServices(services, config, env)
    │                     LoadCompanionBlazorAssemblies(dir)
    │                   _assemblies = AppDomain.CurrentDomain.GetAssemblies()
    │
    ├─[6] await bootstrapper.PostInitialize(moduleManager)
    │       ├── AddRazorPages() + AddApplicationPart foreach UI module
    │       ├── AddModuleAutoMappers(allAssemblies)
    │       └── AddModuleValidators(allAssemblies)
    │
    ├─[7] builder.Build()  → WebApplication (service provider locked)
    │
    ├─[8] await bootstrapper.ConfigureApplication(app, moduleManager)
    │       ├── ApplyMigrations(app)
    │       │     foreach IModuleDbMigrator in DI:
    │       │       if not applied: migrator.ApplyMigrations(serviceProvider)
    │       ├── await moduleManager.InitializeRuntimeAsync(app.Services)
    │       │     ├── ExecuteSettingProviders(provider)
    │       │     │     foreach ISettingDefinationProvider in module assemblies:
    │       │     │       await instance.Define(context)
    │       │     └── ExecuteNavProviders(provider)
    │       │           providers = GetServices<INavigationProvider>()
    │       │           _menuItems = providers.SelectMany(p => p.GetMenuItems())
    │       ├── app.UseRouting() / UseAuth / UseAntiforgery / MapControllers
    │       ├── foreach Action<IEndpointRouteBuilder> in DI: endpointBuilder(app)
    │       └── app.MapRazorComponents<App>()
    │             .AddAdditionalAssemblies(modules + blazorCompanions)
    │
    └─[9] app.Run()   ← Kestrel starts serving requests
```

---

## 2. Phase Analysis

### Phase 1 — PreInitialize
**Timing:** Before any plugin loading.  
**Purpose:** Host infrastructure setup. SettingModule and AuthorizationModule registered here as **static** (not plugin) modules.  
**Notable:** Solution-root detection is development-time only — will fail in Docker or published deployment where the `.sln` file is absent unless deployment scripts set the working directory correctly.

### Phase 2 — Initialize (Plugin Loading)
**Timing:** After host infrastructure, before `builder.Build()`.  
**Purpose:** Load all plugin assemblies and call `IModule.RegisterServices`.  
**Error handling:** Exceptions are caught per-plugin, logged to `Console.WriteLine`, execution continues. A broken plugin is silently skipped — **the host will start without it** with no health signal.

### Phase 3 — PostInitialize
**Timing:** After plugin service registration, before `builder.Build()`.  
**Purpose:** Cross-cutting concerns that need all module assemblies known: AutoMapper, FluentValidation, Razor application parts.

### Phase 4 — ConfigureApplication (Post-Build)
**Timing:** After `builder.Build()` — full service provider available.  
**Purpose:** Database migrations, runtime provider execution (settings, nav), HTTP middleware pipeline, endpoint registration.  
**Important:** This is the only phase that operates on the built `IServiceProvider`.

---

## 3. Initialization Hooks

| Hook | Implementation | Timing |
|---|---|---|
| `IModule.RegisterServices` | Required — `IModule` interface | Phase 2, before build |
| `ISettingDefinationProvider.Define` | Optional — scan + reflection | Phase 4, after build |
| `INavigationProvider.GetMenuItems` | Optional — DI | Phase 4, after build |
| `IModuleDbMigrator.ApplyMigrations` | Optional — DI (scoped) | Phase 4, after build, before middleware |

There are no `IHostedService`-based module hooks in any module. No module uses `BackgroundService`. Shutdown/dispose hooks are not present.

---

## 4. Module Startup Ordering

Order is determined by **filesystem iteration order** of `AppContext.BaseDirectory/plugins/` subdirectories:

```csharp
// ModuleManager.cs L129
foreach (var dir in Directory.GetDirectories(pluginsDir))
```

On Windows, `Directory.GetDirectories` does NOT guarantee alphabetical order (it is filesystem-dependent). This is a **non-deterministic ordering** that could cause issues if a module's `RegisterServices` depends on a service registered by a preceding module.

No topological sort. No declared dependencies between modules.

---

## 5. Lifecycle State Machine

**Not implemented.** There is no state machine for module lifecycle. Modules have two implied states:

| State | Description |
|---|---|
| `NotLoaded` | Plugin directory not found or DLL missing |
| `Loaded` | Assembly loaded and services registered |

The `IModuleManager.Loaded` boolean (`ModuleManager.cs` L28) represents the overall "all modules loaded" state, not per-module state.

---

## 6. What Happens When a Module Fails During Startup

**Source:** `ModuleManager.cs` L155-161

```csharp
catch (Exception ex)
{
    Console.WriteLine($"Failed to load module from {pluginDll}: {ex.Message}");
}
```

Behavior:
- Exception is swallowed.
- The failed module is NOT added to `_modules`.
- The host continues loading remaining modules.
- The host continues startup.
- No health signal is raised.
- Consumers that resolve services from the failed module will get `InvalidOperationException` from DI at request time.

This is a **silent partial degradation** — the application can start and appear healthy while a critical module is missing.

---

## 7. Shutdown and Disposal

No shutdown hooks are registered by any module. No `IDisposable` cleanup. When `app.Run()` returns (SIGTERM / SIGINT), the host DI container is disposed through the standard ASP.NET Core host shutdown mechanism, which disposes all registered `IDisposable` singleton services.

Modules do not participate in an explicit shutdown sequence.

---

## 8. Cancellation Handling

The `IModule.RegisterServices` interface is synchronous. The `IModuleDbMigrator` interface is also synchronous (`void ApplyMigrations`). `ISettingDefinationProvider.Define` is async but is `await`ed without cancellation token propagation:

```csharp
// ModuleManager.cs L106
await instance.Define(context);
```

No cancellation token is passed to any module lifecycle method.
