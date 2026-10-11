# ADR-002: LMS Integration with Frozen IY.Auth 0.1.2

## Context
IY.Auth 0.1.2 is independently verified and frozen. LMS targets .NET 9 and the complete IY.Auth package family provides `net9.0` assets.

## Decision
`LMS.IdentityModule -> IY.Auth.AspNetCore 0.1.2` is the primary package dependency. Add direct `IY.Auth.Contracts 0.1.2` only where public contracts are compiled against. The transitive package family is Domain, Contracts, Application, EntityFrameworkCore, OpenIddict, and AspNetCore, all version 0.1.2.

No IY.Auth source reference, manual DLL, HintPath, linked source, or product change is allowed for LMS convenience.

## Persistence and Migration
LMS uses PostgreSQL/Npgsql and the `Default` physical database. IY.Auth retains its own IdentityDbContext, migrations, and dedicated identity schema. LMS invokes a module bridge to `IIdentityDatabaseMigrator`; LMS must not invoke `IdentityDbContext.Database.Migrate()`.

## Authentication and Authorization
LMS login/logout presentation calls IWebAuthenticationService. LMS never uses UserManager, SignInManager, or IdentityDbContext for account behavior. Cookie authentication uses `Identity.Application`; bearer authentication uses OpenIddict.Validation.AspNetCore. Custom JWT generation, validation, or token parsing is prohibited.

IY.Auth owns authentication. AuthorizationModule/Casbin owns authorization. Permissions are not copied to claims. Human Casbin subjects are authoritative `sub` Guid values rendered as canonical `D` strings. Existing policies must be deterministically compatible; otherwise stop without creating a mapping table. Client credentials principals never become human LMS users; V1 user-scoped Casbin applies only to humans.

## Claims and Result Semantics
No LMS claims contributor is added without a demonstrated consumer. Preserve `sub`. Unknown, wrong-password, and disabled login outcomes map to generic InvalidCredentials; locked maps to UserLockedOut.

## Configuration
Configuration must cover connection/schema, issuer/endpoints, cookie behavior, and certificate material. Development-only certificates are allowed only in Development; production fails closed. Secrets never belong in source configuration.
