# ADR-010: PostgreSQL Execution / Connection Ownership

## Context
`IY.Reporting.SqlKata` requires an `IDbConnection` to execute queries against the PostgreSQL database.

## Decision
Connection ownership: DEFERRED TO LMS-REPORTING-005.

Options to evaluate during implementation:
A — Dedicated NpgsqlConnection from host configuration.
B — LMS infrastructure connection abstraction if one exists.
C — `ReportingDbContext.Database.GetDbConnection()`.
