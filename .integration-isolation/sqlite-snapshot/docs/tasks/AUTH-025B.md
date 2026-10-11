# AUTH-025B: Integrate IY.Auth Bearer Authentication for LMS APIs
Status: BLOCKED BY GATE L3. Gate: L4.

## Objective and Why
Protect LMS APIs with proven OpenIddict validation while preserving cookie coexistence.

## Projects and Files
IdentityModule registration and affected Host/API endpoints; inspect `src/LMS.Host/PermissionsController.cs` and current controller mapping.

## Prerequisites
L3 accepted; Host middleware L1 accepted.

## Implementation
Use IY.Auth/OpenIddict validation surface and explicit endpoint authentication semantics. Keep Host middleware execution generic; avoid a second token implementation.

## Constraints and Security
No custom JWT creation/validation/parser, no bearer-to-cookie conversion, no ambiguity between cookie and bearer schemes.

## Verification and Behavioral Tests
Anonymous API returns 401; invalid bearer returns 401; valid IY.Auth token produces authenticated request; cookie flow remains functional.

## STOP Conditions
Stop on scheme-selection ambiguity or a need for custom token processing.

## Deliverables and Completion Evidence
API auth configuration, integration test output, scheme evidence, execution-state update.
