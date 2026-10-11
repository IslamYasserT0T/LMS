# ADR-004: Reporting Module Boundary

## Context
LMS modules can be multi-project (like SampleModule) or single-project (like LMS.IdentityModule). We need to determine the boundary for `LMS.ReportingModule`.

## Decision
Create the smallest viable boundary: a single project `LMS.ReportingModule` with internal folders for API, Domain, Infrastructure, and UI. This follows the `LMS.IdentityModule` convention and avoids over-layering for a thin adapter.
