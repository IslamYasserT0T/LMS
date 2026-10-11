# AUTH-024A: Verify LMS Claims Requirement
Status: BLOCKED BY AUTH-023A. Gate: L4.

## Objective and Why
Determine whether a concrete LMS consumer needs an IY.Auth claims contributor. Current decision is no contributor by default.

## Projects and Files
IdentityModule and actual identified consumer projects only; inspect claims consumers before adding files.

## Prerequisites
AUTH-023A complete; IY.Auth composition available.

## Implementation
Search concrete LMS identity/claims consumers. If none require additional claims, record `NO CONTRIBUTOR REQUIRED`. If one exists, add only the minimal contributor using authoritative data.

## Constraints and Security
Preserve `sub`; never add permissions, roles, tenant, organization, employee, department, or company claims speculatively. Respect reserved-claim protection and security-stamp exclusion.

## Verification and Behavioral Tests
Test only demonstrated claim behavior; assert `sub` remains authoritative and reserved claims cannot be overwritten.

## STOP Conditions
Stop if a required value has no authoritative source or requires a broad claim model.

## Deliverables and Completion Evidence
Search result and either no-code decision or minimal contributor plus behavioral evidence; execution-state update.
