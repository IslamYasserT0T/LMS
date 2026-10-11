# AUTH-026A: Derive Casbin Subject from Authenticated Human Identity
Status: BLOCKED BY GATE L4. Gate: L5.

## Objective and Why
Replace hard-coded `alice` in `src/LMS.Host/PermissionsController.cs` with the authoritative human `ClaimsPrincipal.sub` rendered as canonical Guid `D`.

## Projects and Files
`shared/Modules/AuthorizationModule`; `src/LMS.Host/PermissionsController.cs` or replacement enforcement boundary; Casbin policy fixtures/tests.

## Prerequisites
L4 accepted; policy data inspected.

## Implementation
Extract authenticated `sub` at authorization enforcement boundary; validate/format Guid consistently; pass it to Casbin. Convert only policies deterministically compatible with that subject convention.

## Constraints and Security
AuthorizationModule must not reference IdentityDbContext, UserManager, SignInManager, or OpenIddict internals. Do not create mapping storage or copy permissions into claims.

## Verification and Behavioral Tests
Anonymous returns 401; authenticated allowed is successful; authenticated denied is 403; no production hard-coded `alice`; Guid format proof.

## STOP Conditions
Stop for architecture approval if persisted policies cannot be deterministically converted from their existing subject values.

## Deliverables and Completion Evidence
Subject resolver/enforcement update, policy compatibility record, authorization test output, execution-state update.
