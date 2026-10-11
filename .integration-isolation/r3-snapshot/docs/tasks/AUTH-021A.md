# AUTH-021A: Remove Temporary Provider Lifecycle Dependence
Status: COMPLETE. Gate: L1 (In Progress).

## Objective and Why
Inspect then remove the `builder.Services.BuildServiceProvider()` use in `src/LMS.Host/Program.cs` while preserving dynamic module discovery/registration. A temporary provider can diverge from the final application container.

## Projects and Files
`src/LMS.Host`; `Program.cs`, `Services/Bootstrapper.cs`, `Services/ModuleManager.cs`, `Contracts/IModuleManager.cs`.

## Prerequisites
Current host builds. Read ADR-001 and current module startup code.

## Implementation
Determine why the early provider obtains IModuleManager. Replace it with an explicit pre-build lifecycle that preserves the same manager state in the final container. Do not redesign plugins. Ensure no production `BuildServiceProvider` remains in this lifecycle.

## Constraints and Security
Host remains orchestration-only; add no IY.Auth dependency or authentication registration. Preserve configuration and module loading behavior.

## Verification and Behavioral Tests
Build Host; start it; confirm modules are discovered, UI modules are added, migrations still enumerate, and only final DI services serve runtime requests.

## STOP Conditions
Stop if removal needs a plugin architecture redesign, changes module contracts beyond the approved boundary, or cannot preserve discovery.

## Deliverables and Completion Evidence

### Files Changed
- `src/LMS.Host/Contracts/IModuleManager.cs`: Added `InitializeRuntimeAsync(IServiceProvider serviceProvider)` for post-build runtime initialization.
- `src/LMS.Host/Services/ModuleManager.cs`: Removed `services.BuildServiceProvider()` from `LoadModules`; implemented `InitializeRuntimeAsync` for setting and navigation providers using the final runtime container; preserved dynamic module discovery, loading, and registration.
- `src/LMS.Host/Services/Bootstrapper.cs`: Exposes `ModuleManager` property; registers instance singleton `AddSingleton<IModuleManager>(ModuleManager)` to ensure identical instance in the DI container; invokes `InitializeRuntimeAsync(app.Services)` during `ConfigureApplication`.
- `src/LMS.Host/Program.cs`: Replaced early `builder.Services.BuildServiceProvider().GetRequiredService<IModuleManager>()` call with `bootstrapper.ModuleManager`.

### Previous Lifecycle
1. `PreInitialize` registered `AddSingleton<IModuleManager, ModuleManager>()`.
2. Temporary `BuildServiceProvider()` call 1 in `Program.cs` to resolve `IModuleManager`.
3. `Initialize(moduleManager)` -> `LoadModules` called temporary `BuildServiceProvider()` call 2 to run `ExecuteSettingProviders` and `ExecuteNavProviders`.
4. `PostInitialize` added further MVC parts, AutoMappers, and validators to `IServiceCollection`.
5. `builder.Build()` constructed a second, final `IServiceProvider`, causing divergence with singleton state.

### New Lifecycle
1. `bootstrapper = new Bootstrapper(builder)` creates single `ModuleManager` instance.
2. `PreInitialize` registers `_builder.Services.AddSingleton<IModuleManager>(ModuleManager)`.
3. `Program.cs` accesses `bootstrapper.ModuleManager` without early container creation.
4. `Initialize(moduleManager)` -> `LoadModules` discovers modules, instantiates them, and registers services into `IServiceCollection` without building a service provider.
5. `PostInitialize(moduleManager)` completes pre-build registrations.
6. `app = builder.Build()` creates the single, authoritative `IServiceProvider`.
7. `ConfigureApplication(app, moduleManager)` applies migrations and runs `moduleManager.InitializeRuntimeAsync(app.Services)`.

### BuildServiceProvider Result
- Production Host `BuildServiceProvider` occurrences: 0.
- Solution-wide `BuildServiceProvider` occurrences: 0.

### Build Result
- `dotnet build src/LMS.Host/LMS.Host.csproj -c Debug`: 0 Errors, 37 Warnings (pre-existing EF Core version conflicts & AutoMapper advisory).
- `dotnet build src/LMS.Host/LMS.Host.csproj -c Release`: 0 Errors, 126 Warnings (pre-existing conflicts across modules).

### Startup Result
- Host started via `dotnet run --project src/LMS.Host/LMS.Host.csproj --no-build`.
- Merged manifests written to `manifest`.
- Discovered and loaded dynamic plugins:
  - `SampleModule.API`
  - `SampleModule.Application`
  - `SampleModule.Core`
  - `SampleModule.Domain`
  - `SampleModule.Infrastructure`
  - `SampleModule.Ui`
- All module services registered.
- Single DI container built successfully.
- Startup reached `ConfigureApplication` / `ApplyMigrations`.

### Tests
- `dotnet test LMS.sln`: 1 test passed, 0 failed (`SampleModule.Test`).
- Note: No pre-existing automated host lifecycle integration test was present. Runtime startup verification was used.

### Remaining Findings
- FOLLOW-UP SECURITY: `src/LMS.Host/appsettings.json` contains committed credentials for local PostgreSQL. Addressed in AUTH-021C.