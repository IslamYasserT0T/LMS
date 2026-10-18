# LMS-D13 — Architectural Risks

**Workstream:** LMS-D13  
**Risk classification:** CRITICAL / HIGH / MEDIUM / LOW

---

## Risk 1 — Silent Module Load Failure (CRITICAL)

**Source:** `ModuleManager.cs` L155-161

```csharp
catch (Exception ex)
{
    Console.WriteLine($"Failed to load module from {pluginDll}: {ex.Message}");
}
```

**Description:** If a module DLL is corrupt, missing dependencies, or throws during `RegisterServices`, the exception is swallowed. The application starts without the module. No health degraded signal is raised.

**Impact:** 
- Authentication module failure → unauthenticated application serving requests.
- Persistence module failure → DI resolution exceptions at request time instead of startup.
- Silent data loss if a critical business module is missing.

**Mitigation for IY.Modules:** Configurable failure policy per module: `Fatal` (abort startup), `Quarantine` (mark degraded, skip), `Warn` (current LMS behavior). At minimum, register a degraded health check.

---

## Risk 2 — Non-Deterministic Module Loading Order (HIGH)

**Source:** `ModuleManager.cs` L129 — `Directory.GetDirectories(pluginsDir)`

**Description:** Module loading order is filesystem-dependent. If Module B's `RegisterServices` depends on a service registered by Module A (e.g., a singleton registered by A that B resolves during factory setup), the result depends on which directory letter-sorts first.

**Impact:** Intermittent DI failures in specific deployment environments.

**Mitigation for IY.Modules:** Topological sort based on declared `Dependencies` in manifest JSON before loading begins.

---

## Risk 3 — No Assembly Isolation (HIGH)

**Source:** `Assembly.LoadFrom` (ModuleManager.cs L137) into default ALC.

**Description:** All module assemblies share the host's type universe. A module shipping a conflicting version of a shared library will silently use the already-loaded (host) version, which may be incompatible.

**Impact:** `MissingMethodException`, `InvalidCastException`, or subtle behavioral bugs from version skew. The regression test (`PluginDeploymentRegressionTests.cs` L115-128) partially mitigates this for `DI.Abstractions` only.

**Mitigation for IY.Modules:** Per-module `AssemblyLoadContext` (non-collectible v1, collectible v2).

---

## Risk 4 — Shared.Core Hard Dependency on SettingModule and AuthorizationModule (HIGH)

**Source:** `shared/Shared.Core/Shared.Core.csproj` L17-18

```xml
<ProjectReference Include="..\Modules\SettingModule\SettingModule.csproj" />
<ProjectReference Include="..\Modules\AuthorizationModule\AuthorizationModule.csproj" />
```

**Description:** `Shared.Core` is supposed to be a portable contracts library. Adding direct project references to domain-specific modules (`SettingModule`, `AuthorizationModule`) defeats this — any project referencing `Shared.Core` must also compile with Casbin, SettingModule persistence, etc.

**Impact:** `IY.Modules` cannot reference `Shared.Core` as a clean abstraction layer. Increases transitive dependency footprint.

**Mitigation for IY.Modules:** Extract `IModule`, `IUiModule`, DI markers into a standalone `IY.Modules.Abstractions` NuGet package with no domain dependencies.

---

## Risk 5 — ModuleManager Hard Coupling to SettingDbContext (MEDIUM)

**Source:** `ModuleManager.cs` L84

```csharp
var dbContext = scope.ServiceProvider.GetRequiredService<SettingDbContext>();
```

**Description:** `ModuleManager.ExecuteSettingProviders` directly resolves `SettingDbContext`. If `SettingModule` is removed or renamed, `ModuleManager` breaks at runtime.

**Impact:** The module infrastructure is not truly generic — it is wired to a specific module's persistence layer.

**Mitigation for IY.Modules:** Replace `ExecuteSettingProviders` with a generic `IModuleStartupHook` interface that modules implement. The manager invokes hooks without knowing their internal persistence.

---

## Risk 6 — Solution-Root Walk for Development Deployment (MEDIUM)

**Source:** `Bootstrapper.cs` L46-52

```csharp
while (dirInfo != null && !File.Exists(Path.Combine(dirInfo.FullName, "LMS.sln")))
    dirInfo = dirInfo.Parent;
```

**Description:** The bootstrapper walks the filesystem to find `LMS.sln` to locate `ManifestPlugins/`. This works in development but fails in:
- Docker containers (no `.sln` file in container)
- Published deployments (no source tree)
- CI/CD runners without workspace preservation

The fallback is `curDir`, meaning manifest is expected to be at `AppContext.BaseDirectory/manifest`.

**Impact:** Deployment ambiguity. Production behavior depends on whether `manifest` file is deployed correctly, not on code logic.

**Mitigation for IY.Modules:** Explicit configuration: `IY.Modules:ManifestsPath` and `IY.Modules:PluginsPath` config keys. No filesystem walks.

---

## Risk 7 — Casbin Model File Path Hardcoded (MEDIUM)

**Source:** `AuthorizationModuleServiceCollectionExtensions.cs` L40

```csharp
var modelPath = Path.Combine("wwwroot", "plugins", "AuthorizationModule", "casbin_model.conf");
```

**Description:** The Casbin model file path is hardcoded to the plugin's wwwroot structure. If the static file serving configuration changes, the authorizer breaks at first request.

**Impact:** Authorization failure at first request with `Enforcer` creation exception.

**Mitigation:** Configure model path via options or embed the model in the assembly as an embedded resource.

---

## Risk 8 — Dual DbContext Registration (MEDIUM)

Multiple modules register `IBaseDbContext` (scoped) mapping to their own concrete DbContext:
- `SampleModuleServiceCollections.cs`: `services.AddScoped<IBaseDbContext, SampleModuleDBContext>()`
- Potentially others

If two modules both register `IBaseDbContext` as scoped, `GetRequiredService<IBaseDbContext>()` returns the **last registered** implementation. Code that uses `IBaseDbContext` generically will get an unpredictable DbContext.

**Impact:** Silent data access to wrong module's schema.

**Mitigation for IY.Modules:** Modules should not share `IBaseDbContext`. Each module should use its own concrete DbContext directly.

---

## Risk 9 — No ILogger, No Structured Telemetry (LOW for NOW, HIGH at Scale)

All module lifecycle events use `Console.WriteLine`. In containerized/cloud deployments, this means:
- Events not captured by Application Insights, Splunk, or ELK.
- No correlation IDs.
- No structured fields (module name, version, error details as machine-parseable fields).

**Impact:** Unobservable failures in production.

**Mitigation for IY.Modules:** Mandate `ILogger<T>` throughout module infrastructure.

---

## Risk 10 — Plugin Deploy Copies DLLs That Are Already Loaded (LOW)

`ModuleFolderCopier.CopyDirectory` uses `File.Copy(..., overwrite: true)`. If the host is running and a module DLL is copied (e.g., during a CI deployment to a running instance), Windows file locking will cause `IOException`. The application will not fail at this point, but the deployment script may error.

**Impact:** Deployment race condition on Windows.

**Mitigation:** Use shadow copy or ensure host is stopped before deployment on Windows.
