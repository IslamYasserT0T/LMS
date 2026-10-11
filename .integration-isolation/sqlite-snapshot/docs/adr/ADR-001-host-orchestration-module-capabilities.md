# ADR-001: Host Orchestration and Module Capability Dependency Ownership

## Context
`LMS.Host` currently owns `Bootstrapper`, `IModuleManager`, module loading, final HTTP pipeline setup, and generic `IModuleDbMigrator` invocation. Modules expose `IModule.RegisterServices(IServiceCollection, IConfiguration)`.

## Decision
Model B is adopted: LMS.Host owns orchestration dependencies; modules own capability implementation dependencies. `LMS.IdentityModule` owns IY.Auth integration. Host owns `UseAuthentication()` then `UseAuthorization()` execution, but has no direct IY.Auth dependency.

No `HostContext`, `IHostContext`, or application-context abstraction is introduced. `Bootstrapper`, `IModuleManager`, and `IModule` are the existing composition mechanism. A future context must not be a service locator, mutable global state, dependency bag, or module gateway.

## Alternatives
Host directly references IY.Auth: rejected because it makes Host a capability dependency magnet. Shared.Core owns IY.Auth contracts: rejected because they are not shared abstractions. A generic HostContext: rejected because it adds indirection without a demonstrated gap.

## Blue-Team Argument
Host already owns startup lifecycle, configuration/environment, module orchestration, migration invocation, and pipeline ordering. Keeping these there gives one explicit composition root.

## Red-Team Argument
Host already directly registers some concrete modules and temporarily builds service providers. Without discipline it can absorb module implementation dependencies. This ADR prevents that; AUTH-021A removes temporary-provider lifecycle dependence without redesigning plugins.

## Consequences and Dependency Rules
- Host may reference shared orchestration abstractions, not IY.Auth, OpenIddict implementation packages, IdentityDbContext, UserManager, or SignInManager.
- IdentityModule may reference IY.Auth.AspNetCore 0.1.2, IY.Auth.Contracts where compilation needs it, and Npgsql provider APIs.
- AuthorizationModule owns Casbin and cannot reference IY.Auth internals.
- Shared layers contain only genuinely cross-module abstractions; no IY.Auth convenience types.

## Enforcement
AUTH-027A verifies Host-to-IY.Auth references, IY.Auth ProjectReferences, manual DLLs, HintPath usage, and AuthorizationModule-to-IY.Auth references are zero. Changes require a new ADR.
