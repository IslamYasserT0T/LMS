# LMS-D09 — Security and Authorization Boundaries

**Workstream:** LMS-D09  
**Status:** Confirmed — fully traced by source inspection.

---

## 1. Authentication Architecture

### Host-level Registration
`Bootstrapper.PreInitialize` (L73): `_builder.Services.AddAuthentication()`  
This registers the ASP.NET Core authentication middleware without any specific scheme. The actual scheme is provided by the IdentityModule.

### IY.Auth / OpenIddict (IdentityModule)
**File:** `modules/IdentityModule/LMS.IdentityModule/IdentityModuleServiceCollectionExtensions.cs`

```csharp
services.AddIYAuth()
    .UseEntityFrameworkCore(options => { ... })
    .UseOpenIddict(creds => { configuration.GetSection("IYAuth:Credentials").Bind(creds); });
```

- IY.Auth is a private NuGet package providing OpenID Connect / OAuth 2.0 via OpenIddict.
- Credentials (signing keys, encryption keys) are bound from configuration section `IYAuth:Credentials`.
- The IdentityModule registers token/authorization endpoints via:
  ```csharp
  services.AddSingleton<Action<IEndpointRouteBuilder>>(e => e.MapIYAuthEndpoints());
  ```
  These are invoked by the host in `ConfigureApplication`.

---

## 2. Authorization Architecture

### Casbin RBAC
**File:** `shared/Modules/AuthorizationModule/ExtentionMethods/AuthorizationModuleServiceCollectionExtensions.cs`

```csharp
services.AddDbContext<AuthorizationDbContext>(...);
services.AddScoped<IAdapter>(provider => new EFCoreAdapter<int>(dbContext));
services.AddScoped<IEnforcer>(sp =>
{
    var modelPath = Path.Combine("wwwroot", "plugins", "AuthorizationModule", "casbin_model.conf");
    var model = DefaultModel.CreateFromFile(modelPath);
    var adapter = sp.GetRequiredService<IAdapter>();
    return new Enforcer(model, adapter);
});
```

- Casbin is an external, open-source authorization library.
- The Casbin model file is shipped as a static asset at `wwwroot/plugins/AuthorizationModule/casbin_model.conf`.
- The enforcer reads policies from `AuthorizationDbContext` via EF Core adapter.
- The `IEnforcer` is scoped — a new Casbin enforcer is created per request scope.

### Module-specific permissions
No module-specific permission definitions were found in the inspected code. Permissions are stored in the Casbin policy database.

---

## 3. Blazor Authentication State

```csharp
// Bootstrapper.PreInitialize L87
_builder.Services.AddCascadingAuthenticationState();
```

Standard Blazor Server authentication state cascading is registered at the host level.

---

## 4. IY.Auth Theme Provider

```csharp
// IdentityModuleServiceCollectionExtensions.cs L43
services.AddScoped<IY.Auth.Contracts.UI.IAuthThemeProvider, LmsAuthThemeProvider>();
```

`LmsAuthThemeProvider` customizes the IY.Auth Blazor login/logout UI to match LMS branding.

---

## 5. Module Activation and Authentication Impact

**Can module activation affect authentication?**  
Yes — the IdentityModule is responsible for configuring all authentication. If the IdentityModule fails to load (DLL missing, DI registration exception), authentication will not be configured and all authenticated endpoints will fail. However, the host currently swallows module load failures silently, so the application might start and present no login UI without any error.

**Can module activation affect authorization?**  
Yes — AuthorizationModule is statically registered (not a plugin), so its presence is always guaranteed. But if the Casbin model file is absent at `wwwroot/plugins/AuthorizationModule/casbin_model.conf`, the `IEnforcer` factory will throw at first use.

---

## 6. Security Boundaries Between Modules

| Boundary | Status |
|---|---|
| Type system isolation | None — default ALC, all types shared |
| DI container isolation | None — single shared container |
| Database schema isolation | Partial — separate DbContext, schema naming convention |
| File system isolation | None — all plugins in shared `plugins/` directory |
| HTTP isolation | None — all routes in same app |

---

## 7. Proof Harness Security (ReportingModule)

The ReportingModule implements a "proof harness" (debug-only synthetic test services) with explicit security controls:

- Proof services are compiled out in Release via `#if DEBUG` (confirmed by `SecurityIsolationTests.cs`).
- The `ProofReportExecutionContext` uses a non-privileged synthetic identity:
  - `SubjectId = "synthetic-proof-runner-unprivileged"`
  - No admin/privileged roles
  - Claims: `Privilege = "None"`, `Role = "NonPrivilegedProofRunner"`, `Scope = "integration-proof-only"`
- `ProofReportAccessPolicy` is fail-closed — always returns `false` (deny by default).
- `Reporting:EnableIntegrationProof` configuration flag is required to opt in.
- Production environment check: proof services are rejected even if opt-in flag is set to `true` in a Production `IHostEnvironment`.

These controls are verified by `SecurityIsolationTests` (9 test methods).

---

## 8. Release/Debug Build Isolation

`ModuleFolderCopier.CopyFoldersFromManifestAsync` enforces that Release hosts cannot deploy Debug plugin binaries:

```csharp
// ModuleFolderCopier.cs L45-51
if (string.Equals(activeConfig, "Release", StringComparison.OrdinalIgnoreCase))
{
    if (resolvedCopyFolder.Contains("Debug", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"Release host attempted to deploy Debug plugin: {resolvedCopyFolder}");
}
```

Verified by `ModuleFolderCopierTests.CopyFoldersFromManifest_InReleaseMode_RejectsDebugPath`.

---

## 9. Credentials and Secrets

Configuration section `IYAuth:Credentials` is bound at startup:
```csharp
configuration.GetSection("IYAuth:Credentials").Bind(creds);
```

The actual credential values are in `appsettings.Development.json` (not inspected — development secrets) and likely in environment variables or secrets management in production. **No credentials were copied into this document.**

---

## 10. Security Boundaries Needed for IY.Modules Management APIs

If IY.Modules introduces a runtime management API (enable/disable modules, reload), the following boundaries must be enforced:

1. Management endpoints must be protected by a strong authorization policy (not just Casbin role check — a dedicated admin claim).
2. Module upload/replacement must validate DLL signatures or checksums.
3. Release-mode enforcement (current `ModuleFolderCopier` check) must be extended to runtime deployment.
4. A module management action that enables a module must re-run its DI registrations — this conflicts with the immutable service provider constraint. A restart-required policy may be the safest first approach.
5. Audit logging of module lifecycle events is absent from LMS and must be designed into IY.Modules.
