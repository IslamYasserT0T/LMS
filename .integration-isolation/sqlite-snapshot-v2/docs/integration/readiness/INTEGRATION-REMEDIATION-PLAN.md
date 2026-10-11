# Integration Remediation Plan

No roadmap task was implemented by this audit.

## G0 — provenance

* **G0-01 (LMS/IY.Auth/IY.Reporting):** capture clean commits, status, package SHA-256 and feed locations. Non-goal: source changes. Exit: every consumed package is reproducible from a committed revision.

## G1 — IY.Auth verification

* **G1-01 (IY.Auth):** build/package 0.2.0 for net9 and execute a net9 isolated consumer suite. Files likely to change: only CI/test configuration if needed. Prerequisite: G0. Exit: Auth package graph and cookie/bearer/refresh/PKCE evidence is archived.

## G2 — IY.Reporting verification

* **G2-01 (IY.Reporting):** make approved 1.0.1 provenance reproducible, run package dependency scan and consumer suite. Files likely: CI/scripts only. Non-goal: LMS-specific code. Exit: seven fresh packages and hashes match declared release inputs.

## G3 — LMS Release stabilization

* **G3-01 (LMS):** commit/review R3 changes; build a clean Release host and verify every active manifest path, loading and static assets. Prerequisite: user-controlled host process is stopped or isolated build output is used. Exit: Release Production host starts without Debug paths.

## G4 — identity runtime adoption

* **G4-01 (LMS):** update LMS to verified Auth 0.2.0 and implement/configure real stores, certificates/secrets, claims and Casbin mapping. Prerequisites: G0/G1. Non-goal: Reporting implementation. Exit: disposable-DB login/logout/cookie/bearer/unauthorized tests pass.

## G5 — Reporting Gate L2 closure

* **G5-01 (LMS):** independently rerun proof-isolation and Release deployment tests, then browser-test English/Arabic/RTL resources. Prerequisite: G3. Exit: Production proof rejection and localization evidence both pass.

## G6 — LMS-REPORTING-003

* **G6-01 (LMS):** implement authenticated report execution context, Casbin access policy, mandatory scope, approved PostgreSQL executor/schema/persistence and export policy. Prerequisites: G2, G4, G5 and explicit user authorization. Non-goals: generic IY.Reporting changes or unrestricted designer access. Exit: no direct executor bypass and all adapters are scope-bound.

## G7/G8 — integration and release audit

* **G7-01 (LMS):** run isolated disposable-PostgreSQL end-to-end security matrix, including cross-user cache/scope and export negatives. Prerequisite: G6.
* **G8-01 (Independent):** repeat from clean revisions and archive commands/results. Prerequisite: G7.
