# LMS Identity Integration Gates

## L1: Host Authentication Readiness
Tasks: AUTH-021A, AUTH-021B, AUTH-021C. Entry: current Host builds. Acceptance: temporary-provider lifecycle removed; one final DI provider; discovery preserved; authentication precedes authorization; Host has zero IY.Auth references; secure configuration path exists; host builds/starts. Evidence: build/start log and source inspection. Failure: stop before L2.

## L2: IdentityModule Package Composition
Tasks: AUTH-022A, AUTH-022B. Entry: L1 accepted. Acceptance: module exists; AspNetCore 0.1.2 is package-only; no source/manual DLL/HintPath; dynamic discovery and transitive deployment closure work; Host remains clean. Evidence: clean restore/published plugin inspection/startup. Failure: stop before L3.

## L3: PostgreSQL Persistence and Migration
Tasks: AUTH-023A, AUTH-023B. Entry: L2 accepted and PostgreSQL available. Acceptance: Npgsql/Default database, separate identity schema, IIdentityDatabaseMigrator bridge, clean migration, idempotent restart, no direct LMS context migration. Evidence: PostgreSQL integration output. Failure: report `POSTGRESQL: NOT VERIFIED` and stop before L4.

## L4: Web and API Authentication
Tasks: AUTH-024A, AUTH-025A, AUTH-025B. Entry: L3 accepted. Acceptance: anonymous web challenge; valid cookie login; authenticated request; generic unknown/wrong/disabled failures; locked result; logout rejection; anonymous/invalid bearer 401; valid bearer authenticated. Evidence: behavioral suite. Failure: stop before L5.

## L5: Casbin Authenticated Identity
Tasks: AUTH-026A, AUTH-026B. Entry: L4 accepted. Acceptance: no `alice`; human subject from `sub` in Guid `D` format; allowed success, denied 403, anonymous 401; machine principal not treated as human. Evidence: authorization suite and policy compatibility check. Failure: architecture review; no mapping table.

## L6: End-to-End Integration
Task: AUTH-027A. Entry: L5 accepted. Acceptance: package boundary, deployment, PostgreSQL migration, cookie, bearer, Casbin, and security tests all pass. Evidence: final report. Failure: no release.
