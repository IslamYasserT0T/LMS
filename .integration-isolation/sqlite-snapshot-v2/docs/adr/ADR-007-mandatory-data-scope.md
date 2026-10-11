# ADR-007: Mandatory Data Scope

## Context
Reporting must enforce business scopes (e.g. TenantId, OrganizationId).

## Decision
Currently, there is no generic tenancy or business scope in LMS production code. The `IReportDataScopeProvider` will default to returning no mandatory predicates. For the integration proof, we will create a TEST-ONLY trusted predicate using the `Sample` fixture to prove that the mandatory scope is correctly applied and cannot be removed or weakened.
