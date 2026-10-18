# LMS-D06 — Assembly Loading and Isolation

**Workstream:** LMS-D06  
**Status:** Confirmed — no isolation. Verified by source inspection.

---

## 1. Assembly Loading API Used

**Source:** `src/LMS.Host/Services/ModuleManager.cs` L137

```csharp
var assembly = Assembly.LoadFrom(pluginDll);
```

`Assembly.LoadFrom` is the **only** assembly loading API used in LMS. No `AssemblyLoadContext`, no `AssemblyDependencyResolver`, no `Assembly.Load(byte[])` for isolation.

---

## 2. What `Assembly.LoadFrom` Means

`Assembly.LoadFrom` loads the assembly into the **default `AssemblyLoadContext`** (the process-global ALC). Consequences:

- All plugin assemblies share the same type system as the host.
- A type `IModule` loaded from a plugin DLL is the **same** `IModule` as the host's `IModule` — because they share the same ALC.
- Two plugins cannot load different versions of the same dependency (e.g., different versions of `Newtonsoft.Json`) — the first one wins.
- Assemblies cannot be unloaded — the default ALC is non-collectible.
- There is no memory isolation between plugins.

---

## 3. AssemblyLoadContext Usage

**Not found.** Grepped the entire codebase for `AssemblyLoadContext`:

```
grep -r "AssemblyLoadContext" d:\LMS\ (src, shared, modules)
→ No matches found
```

No per-module load context. All assemblies are loaded into the default (host) context.

---

## 4. AssemblyDependencyResolver Usage

**Not found.** No `AssemblyDependencyResolver` usage anywhere in the codebase.

---

## 5. Satellite Assembly Resolution

**Source:** `ModuleManager.cs` L247-286

A custom `AppDomain.CurrentDomain.AssemblyResolve` event handler is registered (once, with double-checked locking). It resolves culture-specific resource DLLs (`*.resources.dll`) from plugin subdirectories:

```csharp
private static Assembly? ResolveSatelliteAssemblies(object? sender, ResolveEventArgs args)
{
    var assemblyName = new AssemblyName(args.Name);
    var cultureName = assemblyName.CultureName;
    if (string.IsNullOrEmpty(cultureName)) return null;

    foreach (var dir in Directory.GetDirectories(pluginsDir))
    {
        // check dir/cultureName/AssemblyName.dll
        // check dir/lang/AssemblyName.dll (language fallback)
    }
    return null;
}
```

This is the **only** custom assembly resolution logic in the codebase. It handles `ar`, `ar-SA`, `en`, `en-US` patterns.

---

## 6. Companion Blazor Assembly Loading

**Source:** `ModuleManager.cs` L178-216

After the primary plugin DLL is loaded, the loader scans the same directory for files ending in `.UI.dll` or `.Blazor.dll`. Each candidate is loaded with `Assembly.LoadFrom` and checked for `IComponent`-implementing types with `RouteAttribute`. Qualifying assemblies are stored separately for Blazor router registration.

This is also into the default ALC — no isolation.

---

## 7. Shared Assembly Resolution

Since all assemblies load into the default ALC, shared assemblies (e.g., `Microsoft.Extensions.DependencyInjection.Abstractions`, `Shared.Core`) are resolved from the host's already-loaded set. The `PluginDeploymentRegressionTests` verify that the correct version of `Microsoft.Extensions.DependencyInjection.Abstractions` (9.x, not 10.x) is resolved:

```csharp
// PluginDeploymentRegressionTests.cs L122-127
Assert.DoesNotContain("Microsoft.Extensions.DependencyInjection.Abstractions/10.", depsContent);
Assert.Contains("Microsoft.Extensions.DependencyInjection.Abstractions/9.", depsContent);
```

This is a **version pinning test** that guards against dependency version skew.

---

## 8. Assembly Unloading

**Not supported.** The default ALC is non-collectible. Assemblies loaded via `Assembly.LoadFrom` cannot be unloaded without restarting the process. LMS has no mechanism for module unloading.

---

## 9. Runtime Replacement

**Not supported.** No hot-reload, no restart-less replacement. A module update requires a process restart and re-execution of the full startup sequence including manifest processing and file copying.

---

## 10. Collectible Load Contexts

**Not used.** LMS does not use collectible `AssemblyLoadContext` instances. There is no infrastructure for isolating assemblies that could be unloaded.

---

## 11. Assembly Version Conflicts

With all plugins in the default ALC, version conflicts are handled by the CLR's default binding rules:
- First assembly with a given name loaded wins.
- If a plugin ships a different version of a shared library that the host already loaded, the host version is used.
- This is a known risk: if `LMS.ReportingModule` ships `Newtonsoft.Json 13.0.3` but the host has `13.0.1` loaded, the plugin will use `13.0.1`.

The `.deps.json`-based regression test (`PluginDeploymentRegressionTests.cs` L115-128) partially mitigates this by asserting that the expected version of `DependencyInjection.Abstractions` is present.

---

## 12. Summary

| Capability | Status |
|---|---|
| Default ALC usage | Yes — confirmed |
| Per-module ALC | Not implemented |
| AssemblyDependencyResolver | Not implemented |
| Collectible contexts | Not implemented |
| Assembly unloading | Not possible (default ALC) |
| Version conflict protection | Partial (regression test only) |
| Satellite assembly resolution | Implemented (custom AssemblyResolve handler) |
| Runtime assembly replacement | Not implemented |
| Reflection-based type scanning | Yes — `assembly.GetTypes()` |
