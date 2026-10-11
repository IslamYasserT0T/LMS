# LMS-REPORTING-002 Final Gate Closure Review

Audit date: 2026-10-04. Read-only review. No production source, IY.Reporting, or HRMS file changed.

## Decision

**GATE L2 BLOCKED — SECURITY ISOLATION**

`LMS-REPORTING-002.md` claims `#if DEBUG` proof isolation. Current source contains no such condition. `Module.RegisterServices` unconditionally registers fabricated proof identity, proof scope, proof access policy, proof query executor, and proof schema. `IntegrationProof.razor` unconditionally declares `/reporting/integration-proof`.

This makes production capable of resolving `IReportExecutionContext` as subject `proof-only-subject` with role `TestRunner`. Environment setting cannot protect it: `RegisterServices` receives configuration but never checks environment, build configuration, authenticated caller, or deployment setting. Route has no authorization or runtime environment guard. Proof code compiles into module and deployed Debug plugin output.

## Evidence matrix

| Gate | Result | Evidence |
|---|---|---|
| Package adoption | PARTIALLY VERIFIED | Module csproj uses IY.Reporting, AspNetCore, Blazor `1.0.1`. Deployed deps resolves all three `1.0.1`. |
| Deployed dependency identity | FAILED | Debug plugin DI Abstractions assembly identity `10.0.0.0`; incompatible .NET 9 deployment. No Release ReportingModule plugin exists. |
| Stale IY Reporting `1.0.0` | VERIFIED | Deployed deps has no IY.Reporting `1.0.0`; deployed IY assemblies report `1.0.1.0`. |
| Startup registration evidence | PARTIALLY VERIFIED | `Module.cs` has registrations and startup write; no retained host startup log or running host observed. |
| Proof DI isolation | FAILED | `Module.cs:28-32` always registers proof services. No conditional compilation or environment/access gate. |
| Fabricated identity unavailable in production | FAILED | `ProofReportExecutionContext.cs:8-11` returns fixed subject/claims; DI registration unconditional. |
| Proof executor data isolation | VERIFIED | `ProofQueryExecutor` has no database dependency and returns `null`; cannot execute real query. |
| Proof access/scope safety | PARTIALLY VERIFIED | Access always returns `false`; export default remains fail-closed. Scope returns no predicates, safe only while access stays deny-all. |
| Proof route unavailable in Production | FAILED | `IntegrationProof.razor:1` exposes route unconditionally; no authorization or environment guard. |
| Future adapter override safety | NOT VERIFIED | Proof registrations use ordinary `AddScoped`; replacement depends on later ordering, no enforced boundary. |
| Diagnostic code excluded from production | FAILED | Proof classes compile into module; Debug plugin deploy contains module. Release artifact absent. |
| Viewer/Designer runtime | NOT VERIFIED | Document assertions only; no browser capture, server log, or active host process. Source capability is not runtime evidence. |
| Static assets over HTTP | NOT VERIFIED | Assets exist under plugin `wwwroot/_content/IY.Reporting.Blazor`; no observed HTTP status/content type. |
| EN/AR and RTL | NOT VERIFIED | EN/AR satellite assemblies exist; no browser-observed resource resolution or visual RTL test. |
| Premature production scope | VERIFIED | No production schema, Casbin adapter, PostgreSQL execution, saved-report persistence, or real context in module source. |

## Runtime/configuration observations

Host source enables interactive server components, static assets, and Mud providers. This establishes capability, not Viewer/Designer render or event proof. No local host listener existed during review.

Manifest configuration copies Debug output: `ManifestPlugins/ReportingModuleManifest.json` and `src/LMS.Host/manifest` use `modules/ReportingModule/LMS.ReportingModule/bin/Debug/net9.0/plugins`. Debug plugin is current deployment target.

## Exact corrective actions

1. Remove proof services/route from production assembly, or compile only under trusted build symbol excluded from Release. Do not rely only on `ASPNETCORE_ENVIRONMENT`.
2. Before real adapters, remove proof DI registrations. Require explicit host adapters for context, access, scope, schema, executor.
3. Produce clean Release plugin. Remove stale copied assemblies; verify deployed Microsoft.Extensions identities. Eliminate DI Abstractions `10.0.0.0`.
4. Start clean Release host; capture module startup and loader/runtime evidence.
5. Capture browser proof: Viewer, Designer, interactive event, Mud provider, clean browser/server logs.
6. Request `_content/IY.Reporting.Blazor/css/iy-reporting.css` and required assets. Record URL, HTTP 200, content type.
7. Browser-test English/Arabic strings and Arabic `dir="rtl"` layout.

## Scope note

No LMS-REPORTING-003 work performed. Unrelated LMS working-tree changes untouched.
