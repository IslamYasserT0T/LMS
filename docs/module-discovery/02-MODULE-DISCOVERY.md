# LMS-D02 — Module Discovery Mechanism

**Workstream:** LMS-D02  
**Status:** Confirmed implementation — fully traced.

---

## 1. Discovery Timing

Module discovery occurs **at application startup**, inside `Bootstrapper.Initialize()` which is called from `Program.Main` before `builder.Build()`. It is not a build-time or a runtime (post-startup) operation.

```
Program.Main
  └── bootstrapper.PreInitialize()      // manifests merged, plugins copied
  └── bootstrapper.Initialize(manager) // assembly loading + service registration
  └── bootstrapper.PostInitialize(...)  // AutoMapper, Validators, Blazor parts
  └── builder.Build()
  └── bootstrapper.ConfigureApplication(app, manager)  // middleware, endpoints, migrations
```

---

## 2. Discovery Mechanism — Two Stages

### Stage 1 — Manifest Processing (Bootstrapper.PreInitialize)

**Source:** `src/LMS.Host/Services/Bootstrapper.cs` L37–104  
**Source:** `src/LMS.Host/Services/ModuleManifestsManager.cs`  
**Source:** `src/LMS.Host/Services/ModuleFolderCopier.cs`

#### Step 1a — Solution root walk
The bootstrapper walks directory ancestors from the current working directory until it finds `LMS.sln`:
```csharp
// Bootstrapper.cs L46-52
var dirInfo = new DirectoryInfo(curDir);
while (dirInfo != null && !File.Exists(Path.Combine(dirInfo.FullName, "LMS.sln")))
    dirInfo = dirInfo.Parent;
var solutionRoot = dirInfo?.FullName ?? curDir;
```
This is a development-time coupling — the host must be run from within the repository.

#### Step 1b — Manifest merge
`ModuleManifestsManager.ProcessManifests` reads all `.json` files from `ManifestPlugins/` (recursively), substitutes `{Configuration}` / `$(Configuration)` tokens with the active build configuration (`Debug` / `Release`), merges them into a single `ModulesRoot` object, and writes it to `src/LMS.Host/manifest` (or `<cwd>/manifest` in published deployments).

**Token substitution source:** `ModuleManifestsManager.cs` L55-56

#### Step 1c — Plugin folder copy
`ModuleFolderCopier.CopyFoldersFromManifestAsync` reads the merged manifest and, for each `ModuleDefinition.CopyFolder`, copies the published output of that module sub-project into `AppContext.BaseDirectory/plugins/<ModuleName>/`.

Security guards:
- Release mode rejects any `CopyFolder` containing `"Debug"` (throws `InvalidOperationException`).
- Release mode throws `DirectoryNotFoundException` if source folder is absent.
- Path traversal check on every file copied (line 92 of `ModuleFolderCopier.cs`).

---

### Stage 2 — Assembly Loading and Type Scanning (ModuleManager.LoadModule)

**Source:** `src/LMS.Host/Services/ModuleManager.cs` L123–170

```csharp
private void LoadModule()
{
    var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");

    foreach (var dir in Directory.GetDirectories(pluginsDir))
    {
        var dirName = Path.GetFileName(dir);
        var pluginDll = Path.Combine(dir, dirName + ".dll");
        if (File.Exists(pluginDll))
        {
            var assembly = Assembly.LoadFrom(pluginDll);   // L137

            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t)
                         && !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (var moduleType in moduleTypes)
            {
                if (ModuleInfo.TryCreateModule(assembly, out var moduleInfo))
                    RegisterModuleServices(moduleType, moduleInfo);
            }

            LoadCompanionBlazorAssemblies(dir);          // UI assemblies
        }
    }

    _assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
}
```

#### Key observations
1. **Discovery key** — the DLL filename must match the subdirectory name: `plugins/SampleModule.API/SampleModule.API.dll`.
2. **IModule scan** — `assembly.GetTypes()` + `typeof(IModule).IsAssignableFrom(t)` — reflection-based, no attributes.
3. **Single IModule per assembly** — `ModuleInfo.TryCreateModule` returns on the first match.
4. **No attribute metadata** — module name/description are properties on the `IModule` implementation, not attributes.
5. **Ordering** — `Directory.GetDirectories` order (filesystem, non-deterministic).
6. **No dependency graph** — modules are loaded in traversal order regardless of declared dependencies.

---

## 3. Companion Blazor Assembly Discovery

**Source:** `ModuleManager.cs` L178–216

After the primary DLL is loaded, the loader scans the same plugin directory for `*.UI.dll` or `*.Blazor.dll` files. Each candidate is checked for types that implement `IComponent` and carry `RouteAttribute`. Qualifying assemblies are added to `_additionalBlazorAssemblies` for registration with `MapRazorComponents`.

This is a convention-based sub-mechanism: file names must end in `.UI.dll` or `.Blazor.dll`.

---

## 4. Satellite Assembly Resolution

**Source:** `ModuleManager.cs` L247–286

A one-time `AppDomain.CurrentDomain.AssemblyResolve` handler resolves culture-specific resource DLLs from plugin subdirectories. The resolver is registered with double-checked locking using a static boolean flag.

---

## 5. Discovery Classification

| Mechanism | Implemented |
|---|---|
| Assembly scanning | Yes — reflection via `Assembly.GetTypes()` |
| Attribute-based discovery | No |
| Explicit registration | No (no registration list in code) |
| Configuration-based registration | Yes — JSON manifests |
| Directory-based loading | Yes — `plugins/` directory structure |
| Module manifests | Yes — `ManifestPlugins/*.json` |
| Build-time discovery | Partial — MSBuild `.targets` publish, not discover |
| Runtime discovery (post-startup) | No |

---

## 6. Discovery Execution Path (Complete Trace)

```
Program.Main
  new Bootstrapper(builder)
  bootstrapper.PreInitialize()
    ModuleManifestsManager.ProcessManifests(manifestSource, manifestDest, "Debug")
      FindManifestFiles(ManifestPlugins/) → *.json
      ReadManifestFile(each) → token substitution → ModulesManifest
      WriteToJsonFile → src/LMS.Host/manifest
    ModuleFolderCopier.CopyFoldersFromManifestAsync(manifest, plugins/, "Debug")
      foreach CopyFolder in manifest → CopyDirectory(source, plugins/Name/)
  bootstrapper.Initialize(moduleManager)
    moduleManager.LoadModules(services, configuration, environment)
      EnsureSatelliteAssemblyResolverRegistered()
      LoadModule()
        foreach dir in plugins/
          Assembly.LoadFrom(dir/Name.dll)
          GetTypes().Where(IModule) → RegisterModuleServices(moduleType, moduleInfo)
          LoadCompanionBlazorAssemblies(dir)
        _assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies())
```
