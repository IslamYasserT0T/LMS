# LMS-REPORTING-002-R1 Independent Closure Verification

Audit date: 2026-10-05. Read-only review. No production source, IY.Reporting, or HRMS file changed.

## Verdict

**GATE L2 BLOCKED — SECURITY ISOLATION**

R1 correctly excludes proof code from the compiled Release module. It does not securely isolate the deployed harness: production manifest configuration still copies the **Debug** plugin path, and Debug assembly metadata contains all proof services and `IntegrationProof`. `#if DEBUG` is a build switch, not a runtime environment restriction. A Debug plugin can therefore be deployed while `ASPNETCORE_ENVIRONMENT=Production`, with no independent route/authentication restriction.

## Evidence matrix

| Requirement | Result | Evidence |
|---|---|---|
| Release proof identity/access/scope/executor/schema absent | VERIFIED | Metadata inspection of `modules/ReportingModule/LMS.ReportingModule/bin/Release/net9.0/LMS.ReportingModule.dll`: only `Class1` and `Module`; no `Proof*` or `IntegrationProof` types. Binary string scan also found no proof markers. |
| Release proof route absent | VERIFIED | Release metadata has no `LMS.ReportingModule.Ui.Pages.IntegrationProof`; no `/reporting/integration-proof` marker. |
| Debug proof implementations present | VERIFIED | Debug metadata contains `ProofReportExecutionContext`, `ProofReportAccessPolicy`, `ProofReportDataScopeProvider`, `ProofQueryExecutor`, and `IntegrationProof`. |
| Debug plugin with Production environment safe | FAILED | `ManifestPlugins/ReportingModuleManifest.json` and generated host manifest both select `bin/Debug/net9.0/plugins`. No environment, authorization, or runtime route guard exists. Production environment alone does not remove Debug-compiled proof code. |
| Harness cannot access database/privileged operations | PARTIALLY VERIFIED | `ProofQueryExecutor` has no database dependency and returns `null`; proof access returns `false`. This does not isolate fabricated identity/route from production deployment. |
| Release negative tests run | VERIFIED, insufficient | `dotnet test modules/ReportingModule/LMS.ReportingModule.Test/LMS.ReportingModule.Test.csproj -c Release`: exit 0; 1 passed, 0 failed, 0 skipped. |
| Negative tests test intended assembly | PARTIALLY VERIFIED | Direct project reference rebuilt `LMS.ReportingModule` Release net9.0, but test host targets net10.0. |
| Negative tests cover all proof services/types | FAILED | `SecurityIsolationTests.cs` asserts only no `IReportExecutionContext` registration. It does not assert absent proof types, access/scope/executor/schema registrations, Razor route discovery/access, or fail-closed execution. |
| Release deployment DI identity | VERIFIED | Release plugin `Microsoft.Extensions.DependencyInjection.Abstractions.dll` identity is `9.0.0.0`; deps resolves package `9.0.7`; assets have no 10.x reference. |
| Debug deployment DI identity | VERIFIED now, root cause qualified | Current Debug source and host plugin copies report `9.0.0.0`, with deps/assets `9.0.7`. Earlier 10.0.0.0 artifact was stale output, not current IY.Reporting `1.0.1` dependency resolution. |
| Runtime Viewer/Designer/circuit/MudBlazor | NOT VERIFIED | No safe isolated host configuration or browser evidence supplied. No listener found on expected local ports. |
| RCL static asset HTTP responses | NOT VERIFIED | No URL/status/content-type evidence. Host Debug plugin has flattened `iy-reporting.css` and `MudBlazor.min.css`; expected `_content/...` paths are absent after `ModuleFolderCopier` flattens recursive paths. |
| EN/AR resolution and visual RTL | NOT VERIFIED | EN/AR resource DLLs exist; no browser evidence for strings or RTL layout. |
| No premature LMS-REPORTING-003 scope | VERIFIED | Reporting module source contains no real execution context, Casbin reporting policy, PostgreSQL/SqlKata executor, business schema, saved-report persistence, or production export policy. |

## Test assessment

Test command:

```text
dotnet test D:\LMS\modules\ReportingModule\LMS.ReportingModule.Test\LMS.ReportingModule.Test.csproj -c Release --logger "console;verbosity=minimal"
```

Exit code: `0`. Actual result: `1` passed, `0` failed, `0` skipped.

The test executes current Release module source through `RegisterServices`, but its net10.0 test host differs from target net9.0. More importantly, it proves only absence of `IReportExecutionContext` registration. A future regression could retain other proof types, route, schema, executor, scope, or access registration and still pass.

## Deployment and dependency investigation

`LMS.ReportingModule.csproj` declares IY.Reporting, AspNetCore, and Blazor `1.0.1`. `obj/project.assets.json` and Release plugin deps resolve DI Abstractions `9.0.7`; Release/Debug copied assembly identities are `9.0.0.0`. No current Microsoft.Extensions 10.x deployment was found.

Original 10.0.0.0 observation is consistent with stale copied plugin output. R1 clean/rebuilt outputs remove it, but deployment remains fragile: manifest always points at Debug output, and `ModuleFolderCopier.CopyDirectory` flattens file paths by filename. It does not preserve RCL `_content` directory structure.

## Runtime limitation

Host startup invokes migrations against configured PostgreSQL before serving requests. No isolated test database/configuration was provided. Starting it against the configured connection could mutate external data, outside this read-only audit. Runtime, assets, localization, RTL, and interactive circuit are therefore NOT VERIFIED, not passed.

## Required corrective actions

1. Make production manifest/copy target select clean Release plugin output. Reject Debug plugin deployment in production build/release pipeline.
2. Add runtime defense in depth: proof route/services must require trusted test-only deployment control and explicit authorization; build symbol alone is insufficient when Debug artifacts are deployable.
3. Expand Release net9 test coverage to inspect compiled type metadata, all proof registrations, route discovery/access, and fail-closed execution.
4. Preserve recursive paths in `ModuleFolderCopier`; verify RCL asset URLs from running isolated host with HTTP 200 and correct content types.
5. Provision isolated test database/configuration, then capture browser evidence for Viewer, Designer, interactive event, Mud providers, English/Arabic labels, and Arabic RTL layout.

## Scope control

No LMS-REPORTING-003 implementation started. No IY.Reporting or HRMS modification performed by this audit.
