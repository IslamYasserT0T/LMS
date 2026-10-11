# AUTH-026B: Preserve Human and Client Principal Separation
Status: BLOCKED BY AUTH-026A. Gate: L5.

## Objective and Why
Verify client-credentials principals never become fake human LMS users in Casbin authorization.

## Projects and Files
AuthorizationModule enforcement boundary and its tests; inspect only affected API policy paths.

## Prerequisites
AUTH-026A complete.

## Implementation
Recognize human subject eligibility from authenticated principal semantics. Apply V1 user-scoped Casbin only to humans. Leave machine authorization as a future explicit capability unless an existing feature proves otherwise.

## Constraints and Security
Do not create generalized service-principal authorization, identities, mappings, or user Guids.

## Verification and Behavioral Tests
Human principal reaches Guid subject flow; client-credentials principal is not interpreted as a human; existing explicit machine behavior is unchanged or documented.

## STOP Conditions
Stop and report separately if an existing feature requires machine-specific Casbin policy semantics.

## Deliverables and Completion Evidence
Principal classification evidence, tests, any separate machine-policy finding, execution-state update.
