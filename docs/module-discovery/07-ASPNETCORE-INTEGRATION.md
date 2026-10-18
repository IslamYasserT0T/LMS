# LMS-D07 — ASP.NET Core Integration

**Workstream:** LMS-D07  
**Status:** Confirmed implementation — fully traced.

---

## 1. Controllers

### Registration Pattern A — Host-level scan
`Bootstrapper.PreInitialize` calls `_builder.Services.AddControllers()` (line 76). This scans only host-resident assemblies at build time.

### Registration Pattern B — Module application parts
During `IModule.RegisterServices`, a module can add itself as an application part:

```csharp
// SampleModule.API/Module.cs L33
services.AddControllers().AddApplicationPart(typeof(SampleController).Assembly);
```

This is a standard ASP.NET Core MVC mechanism that allows plugin assemblies to contribute controllers. The call is safe to make multiple times.

Additionally, `Bootstrapper.PostInitialize` adds UI module assemblies as application parts:
```csharp
// Bootstrapper.cs L116-119
foreach (var assembly in modulesUi)
    mvcBuilder.AddApplicationPart(assembly.Assembly);
```

---

## 2. Minimal API Endpoints

No module uses Minimal API (`app.MapGet`, `app.MapPost`). The IdentityModule uses a delegate pattern to register its token/authorize endpoints:

```csharp
// IdentityModuleServiceCollectionExtensions.cs L47
services.AddSingleton<Action<IEndpointRouteBuilder>>(endpoints => endpoints.MapIYAuthEndpoints());
```

The host iterates all registered `Action<IEndpointRouteBuilder>` delegates in `Bootstrapper.ConfigureApplication` (L184-187):
```csharp
foreach (var endpointBuilder in app.Services.GetServices<Action<IEndpointRouteBuilder>>())
    endpointBuilder(app);
```

This is an indirect registration pattern — the module registers a factory singleton, and the host invokes it post-build. This decouples the module from the host's `WebApplication` type.

---

## 3. Middleware

No module registers middleware via `IApplicationBuilder.Use*`. The middleware pipeline is fully managed by the host in `Bootstrapper.ConfigureApplication`:

```
app.UseRouting()
app.UseHttpsRedirection()
app.UseStaticFiles()
app.UseStaticFiles (for each plugin wwwroot)
app.MapStaticAssets()
app.UseAuthentication()
app.UseAuthorization()
app.UseAntiforgery()
app.MapControllers()
```

Modules cannot inject middleware — this is a limitation.

---

## 4. Static Files (Plugin wwwroot)

**Source:** `Bootstrapper.cs` L160-175

The host iterates all plugin directories and mounts their `wwwroot` subdirectory as a physical static file provider:

```csharp
foreach (var pluginDir in Directory.GetDirectories(pluginsRoot))
{
    var pluginWwwroot = Path.Combine(pluginDir, "wwwroot");
    if (Directory.Exists(pluginWwwroot))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(pluginWwwroot),
            RequestPath = ""
        });
    }
}
```

This enables modules to ship CSS, JS, and Blazor `_content/` assets.

---

## 5. Authentication Handlers

Authentication is registered at the host level in `PreInitialize` (`AddAuthentication()`). The IdentityModule augments it through `AddIYAuth()` → `UseEntityFrameworkCore()` → `UseOpenIddict()` chain inside `RegisterServices`. OpenIddict installs its own authentication handlers.

Modules cannot independently register new authentication schemes after the host has started.

---

## 6. Authorization Policies

Authorization is provided by Casbin via `AuthorizationModule`, registered statically in `PreInitialize`. The `IEnforcer` is registered scoped in `AuthorizationModuleServiceCollectionExtensions.cs` (L37-46). The Casbin model file is read from `wwwroot/plugins/AuthorizationModule/casbin_model.conf`.

No module-specific authorization policies found (using `services.AddAuthorization(options => options.AddPolicy(...))` pattern). Modules rely on the host-level Casbin enforcer.

---

## 7. Blazor Components

### Route-bearing Blazor pages

Modules contribute Blazor route components through:
1. Module's primary assembly added via `mvcBuilder.AddApplicationPart` (UI modules, Bootstrapper.cs L118).
2. Companion assemblies (`.UI.dll`, `.Blazor.dll`) discovered by `LoadCompanionBlazorAssemblies` and added via `AddAdditionalAssemblies` (Bootstrapper.cs L192-195).

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        moduleManager.GetModules().Select(m => m.Assembly)
            .Concat(moduleManager.GetAdditionalBlazorAssemblies())
            .Distinct()
            .ToArray());
```

The `ReportingModule` has Blazor pages at `/reports`, `/reports/viewer`, `/reports/designer` (confirmed in `ComponentVerificationTests.cs`).

---

## 8. Hosted Services / Background Workers

No module registers `IHostedService` or `BackgroundService`. All module work happens within HTTP requests or startup hooks. This is a gap — modules have no mechanism for background processing.

---

## 9. Localization

Both `LMS.IdentityModule` and `LMS.ReportingModule` call `services.AddLocalization()` in their `RegisterServices`. Since `AddLocalization` is idempotent in ASP.NET Core, duplicate calls are safe. The host also calls it once in `PreInitialize` (L90). Supported cultures: `en-US`, `en`, `ar-SA`, `ar` (configured in Bootstrapper.cs L148-152).

---

## 10. Is Endpoint Registration Static or Dynamic?

**Static.** All endpoints (controllers, Blazor routes, IY.Auth token/authorize endpoints) are registered during the startup sequence before `app.Run()`. There is no mechanism for registering endpoints after the application starts.

---

## 11. Can Modules Change HTTP Capabilities After Startup?

**No.** The middleware pipeline, authentication configuration, and endpoint map are fixed at startup. A module cannot add new routes, middleware, or authentication schemes after `app.Run()` begins.

---

## 12. Swagger / OpenAPI

Swagger is configured by the host in `PreInitialize` (L94-103) and exposed only in Development. Modules do not contribute their own OpenAPI documents.

---

## 13. Summary

| Capability | Status |
|---|---|
| Controllers | Yes — via `AddApplicationPart` per module |
| Minimal API | Indirect — via `Action<IEndpointRouteBuilder>` singleton |
| Middleware contribution | Not supported |
| Static files | Yes — per-plugin wwwroot |
| Authentication handlers | Host-level + OpenIddict via IY.Auth |
| Authorization policies | Host-level Casbin only |
| Blazor components | Yes — via additional assemblies |
| Hosted services | Not implemented |
| Localization | Yes — host + modules (safe duplicate) |
| Dynamic endpoint registration | Not supported |
| Post-startup HTTP capability change | Not supported |
