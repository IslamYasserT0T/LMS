# AUTH-023A: Configure PostgreSQL Identity Persistence and Schema
Status: BLOCKED BY GATE L2. Gate: L3.

## Objective and Why
Configure IY.Auth persistence through IdentityModule against LMS `Default` PostgreSQL database while isolating identity schema.

## Projects and Files
`modules/IdentityModule/LMS.IdentityModule`; registration extension and options; inspect `src/LMS.Host/appsettings.json` and existing Npgsql module registrations.

## Prerequisites
L2 accepted; PostgreSQL environment available.

## Implementation
Use IY.Auth public EF configuration with Npgsql; select the approved dedicated identity schema; retain IY.Auth-owned IdentityDbContext/migrations.

## Constraints and Security
Do not merge contexts, directly configure IdentityDbContext from LMS, use SQLite, or hard-code connection values.

## Verification and Behavioral Tests
Assert provider/schema configuration; run against PostgreSQL; inspect migration target names and isolation from existing LMS contexts.

## STOP Conditions
Stop if package public APIs cannot select a dedicated schema or PostgreSQL cannot be used.

## Deliverables and Completion Evidence
Provider/schema composition, PostgreSQL configuration proof, execution-state update.
