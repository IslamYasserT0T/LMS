# AUTH-027A: Prove End-to-End LMS Identity Integration
Status: BLOCKED BY GATE L5. Gate: L6.

## Objective and Why
Produce final behavioral and architecture evidence for the approved LMS/IY.Auth integration.

## Projects and Files
New or existing LMS integration test project; all affected Host, IdentityModule, AuthorizationModule deployment artifacts; no IY.Auth source changes.

## Prerequisites
L5 accepted; PostgreSQL available.

## Implementation
Run clean restore/build/publish/start/test sequence. Inspect project assets and references. Validate dynamic deployment closure and all authentication/authorization matrices.

## Constraints and Security
Host-to-IY.Auth equals zero; AuthorizationModule-to-IY.Auth internals equals zero; IY.Auth-to-LMS equals zero; LMS IY.Auth ProjectReferences, HintPaths, manual DLLs, linked source equal zero.

## Verification and Behavioral Tests
PostgreSQL migration/restart; cookie login/logout matrix; bearer matrix; Casbin matrix; package/deployment checks; certificate/configuration regression checks.

## STOP Conditions
Stop release on any failed gate, source fallback, package-boundary violation, or missing PostgreSQL proof.

## Deliverables and Completion Evidence
Final command/test results, dependency scans, deployment inventory, gate sign-off, execution-state update.
