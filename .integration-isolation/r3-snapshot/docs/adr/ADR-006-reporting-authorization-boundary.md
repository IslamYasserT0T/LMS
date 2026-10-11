# ADR-006: Reporting Authorization Boundary

## Context
IY.Reporting requires an `IReportAccessPolicy` to authorize operations like View, Design, Save, Delete, and Export.

## Decision
LMS uses Casbin for authorization (`AuthorizationModule`). We will map reporting permissions to the established LMS Casbin abstraction (e.g. `_enforcer.HasPermissionForUser(...)`). V1 permissions will be kept small (e.g. `Reporting.View`, `Reporting.Export`, `Reporting.Source.Samples`).
