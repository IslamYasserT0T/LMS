# AUTH-023B: Bridge IY.Auth Migrations into LMS Orchestration
Status: BLOCKED BY AUTH-023A. Gate: L3.

## Objective and Why
Connect existing Host generic migrator orchestration to IY.Auth `IIdentityDatabaseMigrator` through IdentityModule.

## Projects and Files
`modules/IdentityModule/LMS.IdentityModule/Persistence/IYAuthMigrationBridge.cs`; inspect `shared/Shared.Infrastructure/Persistence/IModuleDbMigrator.cs` and `Bootstrapper.ApplyMigrations`.

## Prerequisites
AUTH-023A complete; PostgreSQL available.

## Implementation
Implement the existing module migrator contract as a thin bridge to the public IY.Auth migrator. Host continues generic enumeration only.

## Constraints and Security
Never call `IdentityDbContext.Database.Migrate()` from LMS. Do not alter IY.Auth migrations or contexts.

## Verification and Behavioral Tests
Run clean PostgreSQL migration; restart and prove idempotence; confirm identity schema only is affected by the bridge.

## STOP Conditions
Report `POSTGRESQL: NOT VERIFIED` and stop L3 if a real PostgreSQL run is unavailable; stop on direct-context migration pressure.

## Deliverables and Completion Evidence
Bridge, clean/restart PostgreSQL logs, schema evidence, execution-state update.
