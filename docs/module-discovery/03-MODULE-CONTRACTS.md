# LMS-D03 — Module Contracts and Abstractions

**Workstream:** LMS-D03  
**Status:** Confirmed — all contracts located in source.

---

## 1. `IModule` — Primary Module Contract

**File:** `shared/Shared.Core/Contracts/IModule.cs`  
**Namespace:** `Shared.Core.Contracts`  
**Assembly:** `Shared.Core`

```csharp
public interface IModule
{
    string Name { get; }
    string Description { get; }
    Type ModuleType { get; }
    bool IsEnabled { get; }
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
    void RegisterServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment? hostEnvironment)
    {
        RegisterServices(services, configuration); // default implementation
    }
}
```

### Responsibilities
- Declare module identity (`Name`, `Description`)
- Declare enabled state (`IsEnabled` — **not enforced by the loader; decorative only**)
- Register all module services into the host DI container

### Observations
- `ModuleType` — returns a `Type` used by the host only to classify UI modules (check `typeof(IUiModule).IsAssignableFrom(module.ModuleType)`). The value is set arbitrarily by each module; it does not carry self-description metadata.
- `IsEnabled` — the loader never reads this property to skip disabled modules (confirmed: `ModuleManager.cs` L140-149). Decoration only.
- `RegisterServices` with `IHostEnvironment` overload added via default interface implementation — backward compatible with modules that implement only the two-parameter overload.

### Consumers
- `ModuleManager.RegisterModuleServices` (L232-244)
- All three plugin modules implement it directly

### Reusability
Highly reusable as-is. The interface is clean and not LMS-specific. It should be extended in IY.Modules with additional lifecycle methods.

---

## 2. `IUiModule` — UI Module Marker

**File:** `shared/Shared.Core/Contracts/IUiModule.cs`  
**Namespace:** `Shared.Core.Contracts`  
**Assembly:** `Shared.Core`

```csharp
public interface IUiModule { }
```

### Responsibilities
Marker interface. Signals to the host that the module exposes Blazor razor components that should be added to MVC application parts.

### How Used
`ModuleManager.RegisterModuleServices` (L240-243):
```csharp
case var _ when typeof(IUiModule).IsAssignableFrom(module.ModuleType):
    _moduleUis.Add(moduleInfo);
    break;
```
The host then calls `mvcBuilder.AddApplicationPart(assembly.Assembly)` for each UI module in `Bootstrapper.PostInitialize` (L116-119).

### Limitation
Only the **entry-point assembly** is added as an application part. Additional companion Blazor assemblies are handled via the separate `LoadCompanionBlazorAssemblies` mechanism.

### Reusability
Reusable as-is. Simple and well-scoped.

---

## 3. `IData` — Empty Marker

**File:** `shared/Shared.Core/Contracts/IData.cs`  
**Namespace:** `Shared.Core.Contracts`

```csharp
public interface IData { }
```

Used as `ModuleType` value in `SampleModule.API/Module.cs` (`public Type ModuleType => typeof(IData);`). Its purpose appears to signal a non-UI, data-only module, but this is not enforced — there is no code path that checks `typeof(IData).IsAssignableFrom(moduleType)`.

### Reusability
Low value. Can be dropped or repurposed in IY.Modules.

---

## 4. `IModuleManager` — Host Orchestration Contract

**File:** `src/LMS.Host/Contracts/IModuleManager.cs`  
**Namespace:** `LMS.Host.Contracts`  
**Assembly:** `LMS.Host`

```csharp
public interface IModuleManager
{
    bool Loaded { get; set; }
    Task LoadModules(IServiceCollection services, IConfiguration configuration, IHostEnvironment? hostEnvironment = null);
    Task InitializeRuntimeAsync(IServiceProvider serviceProvider);
    List<ModuleInfo> GetUiModules();
    List<ModuleInfo> GetModules();
    List<NavMenuItem> GetMenuItems();
    List<Assembly> GetAllAssemblies();
    List<Assembly> GetAdditionalBlazorAssemblies();
}
```

### Responsibilities
- Load all plugin assemblies and register their services
- Execute post-build providers (settings, navigation)
- Expose loaded module descriptors and assembly lists to the host

### Limitations
- Tightly coupled to `LMS.Host` namespace and Blazor-specific concerns
- Returns `List<ModuleInfo>` which exposes `LMS.Host.Services.ModuleInfo` — an LMS-specific type
- No capability to selectively load/unload modules

### Reusability
The concept is reusable; the implementation must be redesigned for IY.Modules.

---

## 5. `ModuleInfo` — Runtime Module Descriptor

**File:** `src/LMS.Host/Services/ModuleInfo.cs`  
**Namespace:** `LMS.Host.Services`  
**Assembly:** `LMS.Host`

```csharp
public class ModuleInfo
{
    public IModule Module { get; private set; }
    public Assembly Assembly { get; private set; }
    // Factory: TryCreateModule(Assembly assembly, out ModuleInfo moduleInfo)
}
```

Holds the loaded `IModule` instance and its assembly. Created via `TryCreateModule` which iterates `assembly.GetTypes()` looking for the first `IModule` implementation.

### Reusability
The concept is extractable. The type is LMS-host-specific only by namespace. Needs enrichment in IY.Modules (state, dependencies, health).

---

## 6. Service Lifetime Marker Interfaces

**File:** `shared/Shared.Core/DependencyInjections/`  
**Namespace:** `Shared.Core.DependencyInjections`

| Interface | Lifetime |
|---|---|
| `IApplicationService` | Base marker for all scannable services |
| `IScopedService` | Scoped |
| `ISingletonService` | Singleton |
| `ITransientService` | Transient |

Used in `SampleModule.Infrastructure.Extensions.ServiceRepository.AddRepositoryServices()` to perform assembly-scan DI registration. The pattern discovers all types implementing `IApplicationService`, determines their lifetime from the specific sub-interface, and registers `IFoo → Foo` by convention (`"I" + serviceType.Name`).

### Reusability
Fully reusable. A clean, convention-driven DI pattern.

---

## 7. `IModuleDbMigrator` — Migration Contract

**File:** `shared/Shared.Infrastructure/Persistence/IModuleDbMigrator.cs`  
**Namespace:** `Shared.Infrastructure.Persistence`

```csharp
public interface IModuleDbMigrator
{
    void ApplyMigrations(IServiceProvider serviceProvider);
    bool IsMigrationApplied(IServiceProvider serviceProvider);
}
```

Registered as scoped by each module that owns a DbContext. Discovered via `GetServices<IModuleDbMigrator>()` in `Bootstrapper.ApplyMigrations`.

### Reusability
Highly reusable with minor improvement (async overloads).

---

## 8. `ISettingDefinationProvider` — Post-Build Hook

**File:** `shared/Modules/SettingModule/Contracts/ISettingDefinationProvider.cs`  
**Namespace:** `SettingModule.Contracts`

```csharp
public interface ISettingDefinationProvider
{
    Task Define(ISettingDefinationContext context);
}
```

Discovered via reflection in `ModuleManager.ExecuteSettingProviders` — scans all loaded module assemblies for implementing types and invokes `Define` after `builder.Build()`.

### Reusability
Concept reusable. In IY.Modules this should be generalized as an `IModuleStartupHook` or `IModuleInitializer`.

---

## 9. `INavigationProvider` — Navigation Hook

**File:** `shared/Shared.Core/INavigationProvider.cs`  
**Namespace:** Shared.Core

```csharp
public interface INavigationProvider
{
    IEnumerable<NavMenuItem> GetMenuItems();
}
```

Discovered via DI (`GetServices<INavigationProvider>()`) in `ModuleManager.ExecuteNavProviders`. Used to build the host navigation menu.

### Reusability
LMS-specific (navigation/UI concern). Not directly relevant to IY.Modules core.

---

## 10. Contract Classification Summary

| Contract | Assembly | Framework-level | LMS-specific | Reusable |
|---|---|---|---|---|
| `IModule` | Shared.Core | Yes | No | Yes |
| `IUiModule` | Shared.Core | Yes (Blazor) | No | Yes |
| `IData` | Shared.Core | Marginal | No | Low |
| `IModuleManager` | LMS.Host | Yes | Partially | Concept only |
| `ModuleInfo` | LMS.Host | Yes | Partially | Concept only |
| `IApplicationService` | Shared.Core | Yes | No | Yes |
| `IScopedService` etc. | Shared.Core | Yes | No | Yes |
| `IModuleDbMigrator` | Shared.Infrastructure | Yes | No | Yes |
| `ISettingDefinationProvider` | SettingModule | Yes (hook) | Partially | Concept |
| `INavigationProvider` | Shared.Core | No (UI) | Yes | No |
