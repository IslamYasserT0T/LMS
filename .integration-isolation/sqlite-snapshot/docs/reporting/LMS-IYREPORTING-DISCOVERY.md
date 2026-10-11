# LMS + IY.Reporting Integration Discovery

## Executive Summary
IY.Reporting is a modular reporting suite ready for LMS integration. This discovery phase investigates how to build a thin adapter (`LMS.ReportingModule`) to safely integrate the suite into the LMS architecture. Based on the current LMS codebase, LMS uses dynamic module loading, Npgsql EF Core, Casbin for authorization, and Blazor Server. 

## Authoritative IY.Reporting Release State
The final release gate (REPORT-020) confirmed that prior security findings (the insecure export default and export callback generation race) have been independently verified as **RESOLVED**. The package's default export policy now fails closed (requiring host explicit authorization), and the `ReportViewer` export callback is correctly generation-guarded.
The authoritative verdict for the packages is: **READY FOR LMS INTEGRATION WITH TRACKED NON-BLOCKING REMEDIATIONS**.

## LMS Architecture
LMS is a modular monolithic application running .NET 9. The Host (`LMS.Host`) is a thin entry point that registers common infrastructure and loads modules dynamically from a `plugins` directory.

## Module Loader
LMS uses a dynamic module loader that scans the `plugins` directory for `manifest.json`, dynamically loads assemblies (`Assembly.LoadFrom`), and discovers types implementing `IModule`. The `IModuleManager` manages lifecycle, executing `RegisterServices` for dependency injection. It also discovers UI assemblies, configuration providers, navigation providers, and applies EF Core migrations for each module on startup.

## Current Domain Maturity
LMS is currently an early-stage modular shell. **Only `Sample` exists as an actual business/domain entity.** 
We will NOT invent or design reporting schemas around non-existent future domains (Course, Enrollment, Learner, etc.). 

## Infrastructure Adoption vs Business Reporting
Due to domain immaturity, adoption is split into two phases:
**Phase A — Reporting Infrastructure Adoption**: Prove LMS can correctly consume IY.Reporting using the existing `Sample` model purely as a technical integration fixture.
**Phase B — Business Reporting Sources**: DEFERRED. When real business modules exist, their fields will be classified and actual report schemas will be defined.

## Execution Context
The `ReportExecutionContextFactory` adapter will construct the `IReportExecutionContext`. 
- **SubjectId source**: Extracted from the authenticated LMS principal via `User.FindFirstValue("sub")`.
- **Guid parsing**: Parsed as a canonical Guid string (`"D"`).
- **Leakage**: Do NOT directly depend on OpenIddict types or schemes inside the reporting adapter. The context will carry necessary claims/properties downstream but handle anonymous callers gracefully by failing the execution policy.

## Authorization
LMS uses `Casbin` for authorization (`AuthorizationModule`). 
We will implement an `IReportAccessPolicy` using the LMS authorization abstraction (rather than coupling directly to the raw Casbin `IEnforcer` unless that is the established LMS contract).
For the infrastructure proof, V1 permissions will be kept small and follow LMS conventions:
- `Reporting.View`
- `Reporting.Design`
- `Reporting.Save`
- `Reporting.Delete`
- `Reporting.Export`
- Source-specific access like `Reporting.Source.Samples`

## Mandatory Scope
Currently, there is no generic tenancy or business scope in LMS.
The `IReportDataScopeProvider` will default to returning no mandatory predicates. 
However, for the integration proof, we will create a **TEST-ONLY** trusted predicate using the `Sample` fixture to prove that the mandatory scope is correctly applied and that callers cannot remove or weaken it.

## Cache Decision
LMS shared report-result caching is **DISABLED FOR INITIAL ADOPTION**. 
Without a real business scope, blindly using the `UserId` as a cache partition is overly restrictive, and without it, caching is unsafe. Caching will be revisited when real domain scopes are introduced.

## Logical Schema
The `LMS.ReportingModule` will provide a logical schema provider defining sources, columns, types, and relations. This logical reporting schema is separate from the PostgreSQL execution provider. 

## Sample Technical Source
The `Sample` source will be used strictly as a **TECHNICAL INTEGRATION SOURCE**, NOT A PRODUCTION REPORTING DOMAIN. 
The schema will only define enough columns to prove: projection, filter, sort, pagination, authorization, and PostgreSQL execution.

## PostgreSQL Connection Ownership
LMS uses Npgsql and centralized configuration (`"Default"`).
**Option A**: Reporting creates its own `NpgsqlConnection` from `IConfiguration`.
**Option B**: Reporting obtains connection through an LMS infrastructure abstraction.
**Option C**: Reporting uses `ReportingDbContext.Database.GetDbConnection()`.
*Recommendation*: Evaluate Option A vs C during implementation. Option A avoids EF transaction overhead for read-only bulk queries, while Option C enlists in ambient EF logic if required. The connection ownership, lifetime, and disposal must be explicitly managed.

## Real PostgreSQL Verification Plan
An early implementation gate will prove real PostgreSQL execution using LMS's actual environment to execute `IY.Reporting.SqlKata` queries. It will prove parameterized filtering, sorting, pagination, bounded execution, and test-scope application.

## Saved Report Persistence
A dedicated `ReportingDbContext` will handle saved-report persistence.
- **Invariant**: Report definition persistence is strictly separated from report query execution.
- **Model**: `Id`, `Name`, `DefinitionJson`, `FormatVersion`, `SchemaVersion`, `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`.
- **Ownership**: V1 will implement **private ownership only**. 
- **Security Invariant**: A SavedReport is NEVER an authorization grant. Every execution must evaluate current authorization and current data scope.

## Blazor/RCL Integration
LMS uses Blazor Server (`AddInteractiveServerComponents`). `IY.Reporting.Blazor` components will integrate into this architecture. MudBlazor theme providers and RTL settings will be inherited. An early gate will verify static assets (`_content/IY.Reporting.Blazor/...`) are served correctly.

## Export Integration
The IY.Reporting package now fails closed by default. LMS must explicitly register an `IReportExportPolicy` that checks Casbin (`Reporting.Export`).
- **Formats**: CSV/HTML enabled.
- **Licensing**: EPPlus (Excel) and iText (PDF) remain blocked pending a HUMAN / LEGAL DECISION.

## REST API Strategy
**V1 DEFAULT: Blazor Server integration without independent reporting REST API.**
A REST API is deferred until a concrete external consumer exists, unless actual LMS architectural conventions mandate API-first routing for all modules.

## Localization/RTL
LMS lacks a centralized localization module currently. `IY.Reporting.Blazor` supports RTL and provides EN/AR strings for built-in text. Source-specific labels will be localized in the LMS adapter.

## Compatibility Matrix
- .NET Target: .NET 9 (Compatible)
- EF Core: Compatible
- Npgsql: Compatible
- SqlKata: Compatible
- MudBlazor: Compatible
- Database: PostgreSQL (Compatible)

## Dependency Graph
```text
LMS.Host
   ├── LMS.IdentityModule (IY.Auth)
   ├── AuthorizationModule (Casbin)
   └── LMS.ReportingModule
           ├── IY.Reporting
           ├── IY.Reporting.SqlKata
           ├── IY.Reporting.EntityFrameworkCore
           ├── IY.Reporting.Blazor
```

## Package Gap Classification
| Concern | Classification |
|---|---|
| Fail-closed export | RESOLVED IN IY.Reporting |
| Export callback race | RESOLVED IN IY.Reporting |
| External RCL runtime proof | LMS ADOPTION VERIFICATION |
| Real PostgreSQL runtime proof | LMS ADOPTION VERIFICATION |
| Export format descriptor drift | LMS ADAPTER CONCERN FIRST |
| Exporter licensing | HUMAN / LEGAL |
| Missing LMS business domains | LMS DOMAIN MATURITY / DEFERRED |
| Current empty mandatory scope | VALID CURRENT LMS STATE |

## ADR Drafts
The following ADRs must be drafted during implementation planning:
1. ADR — LMS Reporting Adoption
2. ADR — Reporting Module Boundary
3. ADR — Reporting Execution Context
4. ADR — Reporting Authorization Boundary
5. ADR — Mandatory Data Scope
6. ADR — Reporting Schema Ownership
7. ADR — Saved Report Persistence
8. ADR — PostgreSQL Execution / Connection Ownership
9. ADR — Blazor/RCL Integration
10. ADR — Export Integration
11. ADR — Reporting API Strategy

## Implementation Tasks
**LMS-REPORTING-001**: Module shell + package adoption
**LMS-REPORTING-002**: External RCL runtime proof
**LMS-REPORTING-003**: Execution context adapter
**LMS-REPORTING-004**: Logical schema provider + Sample technical source
**LMS-REPORTING-005**: Real PostgreSQL execution integration
**LMS-REPORTING-006**: Casbin/LMS access policy
**LMS-REPORTING-007**: Mandatory data-scope provider + adversarial integration proof
**LMS-REPORTING-008**: Viewer integration
**LMS-REPORTING-009**: Designer integration
**LMS-REPORTING-010**: Saved-report persistence
**LMS-REPORTING-011**: Explicit export policy + exporter integration
**LMS-REPORTING-012**: Localization / RTL / navigation
**LMS-REPORTING-013**: Security integration tests
**LMS-REPORTING-014**: Final LMS reporting integration audit

## Release Gates
- **Gate L1 — Package Compatibility**: Prove PackageReferences restore, module loads, no conflicts.
- **Gate L2 — RCL Runtime**: Prove Viewer/Designer render, MudProviders work, static assets load, EN/AR/RTL works.
- **Gate L3 — Real Database**: Prove real PostgreSQL execution, parameterization, pagination, bounding.
- **Gate L4 — Security Context**: Prove auth mapping, anonymous rejection, Subject A vs B authorization differs, no OpenIddict leakage.
- **Gate L5 — Mandatory Scope**: Prove trusted test scope is applied, user cannot remove/weaken it.
- **Gate L6 — Saved Reports**: Prove host-owned migration, save/load/delete, versions, current authorization/scope execution.
- **Gate L7 — Export**: Prove default denied, LMS explicit policy required, authorized caller succeeds, maximums enforced.
- **Gate L8 — Final Security Audit**: Adversarially verify complete LMS adapter.

## Risk Register
1. **RCL static assets integration**: High impact, Medium likelihood. Mitigated by Gate L2 early proof.
2. **PostgreSQL execution drift**: High impact, Low likelihood. Mitigated by Gate L3 early proof.
3. **Format Descriptor Drift**: Low impact. UI descriptors may fall out of sync with backend capabilities. Mitigated by UI adapter logic.

## Deferred Business Reporting
Business reporting schemas are completely deferred. We will NOT construct fake schemas for unimplemented domains. Real schemas will be added as business modules (Courses, Enrollments) are established.

## Open Questions
- What is the established LMS convention for wrapping Casbin's `IEnforcer` versus injecting it directly?
- Which PostgreSQL connection strategy (Option A or C) aligns best with LMS infrastructure guidelines?

## Recommended Implementation Order
001 -> 002 -> 003 -> 004 -> 005 -> 006 -> 007 -> 008 -> 009 -> 010 -> 011 -> 012 -> 013 -> 014.

## Final Status
READY FOR LMS REPORTING INFRASTRUCTURE IMPLEMENTATION
*(BUSINESS REPORT SOURCE DESIGN DEFERRED UNTIL REAL LMS DOMAINS EXIST)*
