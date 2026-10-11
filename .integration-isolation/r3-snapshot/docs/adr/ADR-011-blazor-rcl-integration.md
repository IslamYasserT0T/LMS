# ADR-011: Blazor/RCL Integration

## Context
LMS relies on Blazor Server and MudBlazor.

## Decision
`IY.Reporting.Blazor` components will integrate directly into the Blazor Server interactive architecture. They will inherit MudBlazor theme providers and RTL settings from the host. An early implementation gate will verify static assets are served correctly.
