# LMS Identity Integration Architecture

## Approved Boundary
```text
LMS.Host
  Bootstrapper / IModuleManager / configuration / lifecycle
  middleware execution / migration orchestration / readiness
                         |
                         v
LMS.IdentityModule
  IY.Auth.AspNetCore 0.1.2
  IY.Auth.Contracts only where required
  Npgsql composition / migration bridge / login adapter
                         |
                         v
IY.Auth 0.1.2 package family
```

```text
ClaimsPrincipal.sub
  -> canonical Guid "D" subject (human only)
  -> AuthorizationModule enforcement boundary
  -> Casbin
```

## Ownership
- Host: module discovery/lifecycle, configuration/environment, middleware order, migration orchestration, health/readiness, observability composition.
- IdentityModule: IY.Auth package references, EF provider configuration, IIdentityDatabaseMigrator bridge, login/logout adapter, module deployment closure.
- AuthorizationModule: Casbin model, policies, enforcement, authorization semantics.
- Shared layers: existing genuinely shared abstractions only.

## Persistence
Provider is PostgreSQL/Npgsql. Identity shares LMS `Default` physical database but owns a dedicated identity schema and separate IY.Auth IdentityDbContext. Contexts are never merged.

## Current Source Evidence
- `src/LMS.Host/Services/Bootstrapper.cs`: Npgsql `Default`, generic migrator loop, `UseAuthorization()` without authentication.
- `src/LMS.Host/Program.cs`: temporary `BuildServiceProvider` used to obtain IModuleManager.
- `shared/Shared.Core/Contracts/IModule.cs`: module registration contract.
- `src/LMS.Host/PermissionsController.cs`: hard-coded `alice` Casbin subject.
- `shared/Modules/AuthorizationModule/...ServiceCollectionExtensions.cs`: Casbin/Npgsql ownership.

## Constraints
No HostContext. No custom JWT stack. No IY.Auth source references. No Casbin permissions in claims. No identity mapping table unless an ADR is approved. Authentication UI strings must be centralized/resource-ready; no localization system was found, but that is not an integration blocker.

## Security Finding
FOLLOW-UP SECURITY: `src/LMS.Host/appsettings.json` currently contains a connection string with committed credentials. The value is intentionally not reproduced here. AUTH-021C must ensure no new secret is committed and deployment configuration supplies sensitive values outside source. Remediation of the existing secret follows repository secret-management policy and is not silently bundled with authentication behavior changes.
