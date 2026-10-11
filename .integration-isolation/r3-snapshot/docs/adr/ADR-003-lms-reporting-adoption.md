# ADR-003: LMS Reporting Adoption

## Context
LMS needs reporting capabilities. The IY.Reporting suite has passed its final independent release gate and is ready for integration.

## Decision
Adopt IY.Reporting via a thin adapter module (`LMS.ReportingModule`). The adapter will supply schema, execution context, access policy, mandatory scope, provider configuration, and explicit export policy. Business reporting sources are deferred until real LMS domains exist. Sample will be used as a technical integration fixture for Phase A.
