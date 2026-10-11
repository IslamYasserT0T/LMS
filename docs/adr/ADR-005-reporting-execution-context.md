# ADR-005: Reporting Execution Context

## Context
Reporting needs to identify the caller to enforce authorization and mandatory data scopes.

## Decision
The `ReportExecutionContextFactory` adapter will construct `IReportExecutionContext`. The `SubjectId` will be extracted from the authenticated LMS principal via `User.FindFirstValue("sub")` and parsed as a canonical Guid string (`"D"`). We will NOT directly depend on OpenIddict types or schemes inside the reporting adapter. Anonymous callers will be handled gracefully by failing the execution policy.
