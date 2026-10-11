# ADR-008: Reporting Schema Ownership

## Context
Reporting needs a logical schema mapping to the underlying data sources.

## Decision
The logical schema provider will describe logical sources, columns, types, and relations. This is separated from the PostgreSQL execution provider. For the initial infrastructure adoption phase, `Sample` will be used strictly as a TECHNICAL INTEGRATION FIXTURE, not a production reporting domain.
