# Integration Blocker Register

| ID | Severity | Repository | Root cause / component | Evidence | Required fix / acceptance test | Risk | Status |
|---|---|---|---|---|---|---|---|
| AUTH-001 | P1 | LMS | LMS Identity consumes 0.1.2, not approved IY.Auth 0.2.0 | `LMS.IdentityModule.csproj:10-11` vs `IY.Auth/Directory.Build.props:7` | Upgrade only after a fresh 0.2.0 artifact/compatibility gate; isolated net9 LMS login, bearer and migration tests pass | High | Open |
| REPORT-001 | P1 | IY.Reporting | Dirty source lacks fresh 1.0.1 package provenance | `git status --short`; `artifacts/audit-1.0.1` is not tied to current modified source | Clean/commit source, pack seven packages, hash them, run Release + isolated consumer matrix | High | Open |
| LMS-001 | P1 | LMS | Production reporting adapters are absent by design | `Module.cs`; Release security test expects no `IReportExecutionContext` | Implement G6 adapters and prove policy/scope/executor chain against disposable DB | High | Open |
| LMS-002 | P1 | LMS | R3 security/deployment remediation is uncommitted and runtime-unverified | dirty manifest/module/copy changes; test build blocked by PID 26420 | Commit/review R3; release host must start with every active plugin and Production proof opt-in must register nothing | High | Unverified |
| LMS-003 | P1 | LMS | Arabic culture and IY.Reporting resource resolution has no current passing evidence | 2026-10-05 L2 report recorded failure; no new browser run | Isolated host browser test switches culture and verifies Arabic/RTL Viewer and Designer strings/assets | Medium | Open |
| LMS-004 | P2 | LMS | AutoMapper 15.0.1 has high-severity NU1903 advisory | current test-build output | Triage/version remediation in separate dependency task; clean restore/build | Medium | Open |
| AUTH-002 | P2 | IY.Auth | Main sample/consumer verification targets net10.0 | sample and sample-test csproj files | Restore a net9 clean consumer test for LMS adoption gate | Medium | Open |
| REPORT-002 | P2 | IY.Reporting | Package verification script/CI provenance not independently demonstrated | `scripts/verify-packages.ps1`; historical review evidence only | CI invokes pack/hash/consumer verification and archives report | Medium | Open |

Historical L2 findings are not counted as current resolved facts: source remediation exists, but remains unverified until executable evidence is renewed.
