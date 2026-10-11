# Security Acceptance Results

Passed in isolated Debug suite: 22/22 Reporting module tests. Includes proof disabled, proof enabled only in Development, Production Debug-artifact proof rejection, Release proof-type absence, manifest configuration substitution, copy hierarchy and dependency checks.

Implemented controls: authenticated Guid context, Casbin default deny, mandatory `CreatedBy` scope, no direct executor registration, explicit export policy, no Release proof services.

Not executed: Casbin database allow/deny runtime, cross-user data test, scope-negation test, export runtime, cache isolation, malformed token/revocation runtime. Disposable PostgreSQL is required.
