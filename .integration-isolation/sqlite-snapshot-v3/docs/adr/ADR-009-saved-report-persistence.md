# ADR-009: Saved Report Persistence

## Context
Saved reports require persistence separate from dynamic query execution.

## Decision
A dedicated `ReportingDbContext` will handle saved-report persistence with a `SavedReports` table. V1 will implement private ownership only. A SavedReport is NEVER an authorization grant; every execution must evaluate current authorization and current data scope.
