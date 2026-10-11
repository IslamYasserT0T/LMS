# LMS-REPORTING-002-R2 Final Gate L2 Verification

Audit date: 2026-10-05. Independent read-only audit. No production source, IY.Reporting source, or HRMS file was changed.

## Verdict

**GATE L2 BLOCKED — SECURITY ISOLATION**

Release output is clean, and Debug development runtime/asset checks succeeded. Two material blockers remain:

1. A Debug artifact with `Reporting:EnableIntegrationProof=true` under `ASPNETCORE_ENVIRONMENT=Production` registers the proof services. Actual Production-host log: `Integration proof explicitly enabled. Proof harness services registered.` `Module.RegisterServices` contains no environment/production guard. The Razor component denies its UI in Production, but that does not undo already registered fabricated execution-context, access-policy, scope-provider, query-executor, and schema services.
2. A Release Host cannot start because the runtime copier rejects existing Debug paths for Identity/Sample plugins. Reporting resolves Release correctly, but actual Release/Production startup exits before serving the application.

Arabic localization also failed independent browser verification: after the Arabic control action, the displayed culture remained `en-US`; package labels remained English (`VIEWER COMPONENT`, `DESIGNER COMPONENT`, `Filters`). RTL flag changed, but Arabic package-resource resolution was not demonstrated.

## Independent evidence matrix

| Requirement | Result | Evidence |
|---|---|---|
| Release → Release deployment | VERIFIED | Release generated manifest resolves ReportingModule to `modules/ReportingModule/LMS.ReportingModule/bin/Release/net9.0/plugins`. |
| Release rejects Debug plugin | VERIFIED, host-blocking | Release runtime threw `InvalidOperationException` on Debug Identity plugin before startup. It prevents silent Debug copying, but blocks Release Host. |
| Release proof code absent | VERIFIED | PE metadata for Release `LMS.ReportingModule.dll`: only `<Module>`, `Class1`, `Module`; no Proof/IntegrationProof types. |
| Debug without opt-in safe | VERIFIED by test | Debug suite passed service-registration negative test; source gates registration on `Reporting:EnableIntegrationProof`. |
| Debug + opt-in isolated | PARTIALLY VERIFIED | Development Host logged registration, Viewer/Designer ran, access policy returns false and executor is synthetic. Production-environment protection failed. |
| Debug artifact under Production safe | FAILED | Independent Debug/Production Host on `http://localhost:5006` logged proof harness registration with explicit opt-in. |
| Synthetic identity non-privileged | VERIFIED | `synthetic-proof-runner-unprivileged`; claims only `Scope=integration-proof-only`, `Privilege=None`, `Role=NonPrivilegedProofRunner`; partition `none`. |
| Access/export fail closed | VERIFIED | Proof access policy returns `false`; no permissive export policy registered. |
| Synthetic executor isolated | VERIFIED | No connection/configuration dependency; returns empty in-memory `ReportResult`. |
| Tests target net9 | VERIFIED | `LMS.ReportingModule.Test.csproj` targets `net9.0`. |
| Debug tests | VERIFIED | `dotnet test ... -c Debug --no-build`: 18 passed, 0 failed, 0 skipped. |
| Release tests | VERIFIED | Rebuilt Release test project, then `dotnet test ... -c Release --no-build`: 12 passed, 0 failed, 0 skipped. |
| ModuleFolderCopier hierarchy | VERIFIED | Current Debug/Release plugin outputs retain `wwwroot/_content/IY.Reporting.Blazor/css/iy-reporting.css` and MudBlazor path. |
| Path traversal protection | VERIFIED, limited test | Copier preserves `Path.GetRelativePath`, canonicalizes destination, checks destination prefix. Test suite passes. Existing traversal test tests normal input only, not adversarial filesystem/symlink cases. |
| .NET 9 dependency deployment | VERIFIED | Debug and Release plugin assembly identity: DI Abstractions `9.0.0.0`; deps resolves `9.0.7`; IY.Reporting family `1.0.1`; no 10.x or active `1.0.0`. |
| Isolated DB | VERIFIED | Audit Host commands supplied `ConnectionStrings__Default` database `lms_reporting_l2_proof`; startup logs only queried migration histories there. `LMSLocal` was not selected. |
| Real Host startup | VERIFIED for Debug; FAILED for Release | Debug Development Host started on `http://localhost:5005`. Release Production Host failed before listener due rejected Debug Identity plugin path. |
| Dynamic module loading | VERIFIED for Debug | Log loaded `LMS.ReportingModule.dll` from Debug plugin directory; no relevant loader exception. |
| RCL static assets | VERIFIED | HTTP GETs from Debug isolated Host returned HTTP 200: IY CSS `text/css`, 3,024 bytes; Mud CSS `text/css`, 606,040 bytes; Mud JS `text/javascript`, 73,366 bytes; background PNG `image/png`, 378 bytes. |
| ReportViewer | VERIFIED | Browser at `/reporting/integration-proof` rendered Viewer controls and empty synthetic result UI. |
| ReportDesigner | VERIFIED | Browser tab switch rendered `Report Designer`, Saved Reports, and Data Source controls. |
| Interactive Server | VERIFIED | Browser click switched Viewer to Designer without navigation/disconnect; fresh DOM state contained designer UI. |
| English localization | VERIFIED | Browser showed English package labels: Viewer Component, Designer Component, Filters, Refresh. |
| Arabic localization | FAILED | Arabic button changed RTL state only; browser still showed `Current Culture: en-US` and English package labels. |
| RTL | NOT VERIFIED | RTL boolean/direction changed, but Arabic resources failed and narrow browser rendering showed distorted layout. Usable Arabic Viewer/Designer not independently demonstrated. |
| Scope integrity | VERIFIED | No production context adapter, Casbin reporting policy, PostgreSQL/SqlKata executor, business schema, saved-report persistence, production export authorization, or reporting REST API found in ReportingModule source. |

## Commands and runtime results

```text
dotnet build D:\LMS\modules\ReportingModule\LMS.ReportingModule\LMS.ReportingModule.csproj -c Debug
```

Exit `0`; 0 errors. Debug test command exited `0`, 18/18 passed.

```text
dotnet build D:\LMS\modules\ReportingModule\LMS.ReportingModule.Test\LMS.ReportingModule.Test.csproj -c Release
dotnet test D:\LMS\modules\ReportingModule\LMS.ReportingModule.Test\LMS.ReportingModule.Test.csproj -c Release --no-build
```

Exit `0`; 12/12 passed. Build emitted unrelated existing warnings: AutoMapper `NU1903`, EF Relational version conflict `MSB3277`, and IY.Auth `NU1603` resolution warnings.

Debug Host command used environment `Development`, `Reporting__EnableIntegrationProof=true`, URL `http://localhost:5005`, and isolated database `lms_reporting_l2_proof`. It started, dynamically loaded ReportingModule, reported no ReflectionTypeLoadException/FileLoadException/DI activation error, and served tested assets.

Debug artifact Production check used the same isolated database with `ASPNETCORE_ENVIRONMENT=Production`, opt-in true, URL `http://localhost:5006`. It started and logged proof-harness service registration: direct evidence that source lacks the claimed environment gate.

Release Host command used `Production`, opt-in true, isolated database, URL `http://localhost:5007`. It failed before startup because `ModuleFolderCopier` rejects Debug Identity module path. Generated manifest did resolve ReportingModule to Release, but other generated module paths stayed Debug.

## Test adequacy finding

The 18/12 tests are real net9 tests and prove Release metadata/registration conditions. They do **not** prove the claimed production environment guard. `DebugModule_ProofRouteComponent_SecurityGuardValidation` tests a local helper, `EvaluateProofRouteGuard`, rather than invoking `Module.RegisterServices` with a production `IHostEnvironment`; `Module.RegisterServices` accepts no environment and contains no `IsProduction` check. Passing test count therefore does not negate the reproduced production registration.

## Deployment findings

Reporting deployment remediation itself works: placeholder substitution resolves Debug for Debug and Release for Release; Release Reporting plugin output has correct net9 dependencies and preserved RCL tree. `ModuleFolderCopier` now preserves relative paths and avoids filename flattening.

Release deployment is still not end-to-end runnable because other manifest entries remain hard-coded Debug and the new fail-closed copier rejects them. Correct behavior, incomplete release migration.

## Security isolation findings

`#if DEBUG` removes proof code from Release. It does not protect a Debug build running under Production. Because `Module.cs` registers proof services using configuration only, explicit opt-in under Production creates a fabricated `IReportExecutionContext` and related proof services. Component UI denial is insufficient service-boundary isolation. This is a material Gate L2 security failure.

## Runtime, assets, localization, RTL

Development proof runtime, dynamic loading, Viewer, Designer, interactive circuit, and required static HTTP assets are independently verified. Browser Arabic test failed: culture stayed `en-US`, English package strings remained, and visual RTL usability was not established. Implementation-agent screenshots were not used as decisive evidence.

## Required remediation before re-audit

1. Add an actual production-environment gate to proof **service registration**, using a host-environment dependency or equivalent trusted runtime boundary; test the real registration path under Production.
2. Complete Release configuration for every plugin manifest entry or isolate Reporting deployment so Release Host starts with no Debug plugin path.
3. Fix culture switching so a new render resolves `IY.Reporting.Blazor` Arabic resources; re-run browser localization/RTL evidence.
4. Extend tests to invoke real production-environment registration, rather than testing duplicated guard logic.

## Next authorized gate

None. Do not begin LMS-REPORTING-003 until a corrected R3 remediation passes an independent Gate L2 re-audit.
