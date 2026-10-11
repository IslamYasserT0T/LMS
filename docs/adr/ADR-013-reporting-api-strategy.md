# ADR-013: Reporting API Strategy

## Context
Reporting viewer and designer could be powered by a REST API or directly via Blazor Server.

## Decision
No independent Reporting REST API for initial adoption. We will use Blazor Server/application-service integration first. REST API is deferred until a concrete external consumer exists.
