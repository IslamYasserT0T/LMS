# AUTH-021 to AUTH-027 Master Plan

## Scope and Invariants
Integrate frozen IY.Auth 0.1.2 through a dynamic LMS.IdentityModule. Host orchestrates; modules own capabilities. IY.Auth remains external and package-only. PostgreSQL is the only persistence acceptance environment.

## DAG and Critical Path
```text
021A -> 021B -> 021C -> 022A -> 022B -> 023A
                                      |       |\
                                      |       |  -> 024A
                                      |       -> 023B
                                      |            |\
                                      +------------+ -> 025A, 025B -> 026A -> 026B -> 027A
```
Critical path: 021A, 021B, 021C, 022A, 022B, 023A, 023B, 025A/025B, 026A, 026B, 027A. AUTH-024A may run in parallel with AUTH-023B.

## Gates
L1 Host readiness: 021A-C. L2 package composition: 022A-B. L3 PostgreSQL persistence: 023A-B. L4 web/API authentication: 024A, 025A-B. L5 authenticated Casbin identity: 026A-B. L6 complete integration: 027A. See `lms-identity-gates.md`.

## Testing Strategy
Behavioral tests lead: web challenge/login/cookie/logout; generic credentials errors and locked result; bearer anonymous/invalid/valid matrix; Casbin anonymous/allowed/denied matrix; clean PostgreSQL migration and restart; dynamic plugin deployment; package-boundary inspection. SQLite cannot satisfy L3.

## STOP Conditions
Stop rather than redesign plugins if temporary-provider removal requires a plugin redesign. Stop if package public APIs cannot configure a distinct schema, if PostgreSQL is unavailable for L3 evidence, if policy subjects cannot map deterministically from `sub`, or if a new claim/machine authorization model is needed.

## Risk Register
- Host dependency leakage: evidence is existing concrete Host registrations; mitigation is ADR-001 and AUTH-027A reference scans; gate L2/L6.
- Plugin deployment closure: evidence is manifest/copy-based dynamic loading; mitigation is clean publish inspection in AUTH-022B; gate L2.
- Schema/provider compatibility: evidence is all current contexts use Npgsql `Default`; mitigation is clean PostgreSQL migration/restart in AUTH-023B; gate L3.
- Authentication middleware ordering: evidence is current Host has authorization without authentication; mitigation is AUTH-021B behavioral verification; gate L1.
- Casbin subject compatibility: evidence is hard-coded `alice`; mitigation is deterministic Guid `D` conversion check; gate L5.
- Human versus machine identity: mitigation is AUTH-026B principal classification; gate L5.
- Cookie/bearer coexistence: mitigation is separate behavioral matrices; gate L4.
- Certificate and secret deployment: evidence is committed connection credentials in Host configuration; mitigation is configuration contract and secret scan; gate L1.

## Definition of Done
All L1-L6 acceptance criteria pass; no source-based IY.Auth consumption; Host has zero IY.Auth dependencies; IY.Auth has zero LMS dependencies; PostgreSQL, cookie, bearer, Casbin, deployment, and security regressions are evidenced.
