# AUTH-025A: Implement LMS Cookie Login and Logout Adapter
Status: BLOCKED BY GATE L3. Gate: L4.

## Objective and Why
Provide LMS presentation endpoints/pages that use IY.Auth public web authentication behavior.

## Projects and Files
IdentityModule Razor/page or endpoint files; Host component routing only if required by module integration; inspect `src/LMS.Host/Components` conventions.

## Prerequisites
L3 accepted; AUTH-024A complete or recorded no-contributor outcome.

## Implementation
Call IWebAuthenticationService for login and SignOutAsync for logout. Centralize/resource-ready UI strings. Render generic InvalidCredentials for unknown, wrong, and disabled accounts; render locked outcome separately.

## Constraints and Security
No UserManager, SignInManager, IdentityDbContext, custom password validation, or account-existence disclosure. Cookie must be `Identity.Application` through IY.Auth.

## Verification and Behavioral Tests
Anonymous protected page challenges; valid login issues cookie; cookie authenticates; unknown/wrong/disabled are generic; locked is explicit; logout rejects old session.

## STOP Conditions
Stop if implementation requires Identity internals, duplicate cookie setup, or unapproved localization architecture.

## Deliverables and Completion Evidence
Login/logout adapter, behavioral suite result, UI string location, execution-state update.
