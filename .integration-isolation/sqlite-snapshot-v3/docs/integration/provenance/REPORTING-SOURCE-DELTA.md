# Reporting Source Delta

Current `master` checkout has 20 tracked modified files (188 insertions, 37 deletions) and six untracked release/review/script paths. It is not assumed to represent the audited 1.0.1 package artifacts.

| Classification | Paths / evidence |
|---|---|
| Post-approval production modification | `DefaultReportExportPolicy.cs` changes default export from allow to explicit denial; `IdentifierResolver.cs` tightens invalid segment validation; SqlKata changes DI abstractions from 10.0.12 to 9.0.2. These alter behavior/dependency graph and require release evidence. |
| Version/release alignment | Seven package project edits move 1.0.0 to 1.0.1; standalone consumer moves seven package references to 1.0.1 and points NuGet source to `artifacts/audit-1.0.1`. These are consistent with a 1.0.1 release attempt but are uncommitted. |
| Test-only / consumer verification | Blazor viewer tests, clean-consumer execution/export tests, export tests, SqlKata/EF test project updates, standalone verification tests. These do not prove the binary artifacts without an executed clean build. |
| Documentation | `docs/implementation/EXECUTION-STATE.md`; untracked REPORT-020/021 and three review documents. |
| Unfinished / unknown | `scripts/verify-packages.ps1` is untracked. No committed CI invocation, source revision-to-package manifest, or detached signature was found. |

The approved 1.0.1 artifacts may remain a baseline, but current modified source must not be repacked as 1.0.1. After provenance is restored, behavior/dependency changes warrant a new version according to the release policy (at minimum a distinct prerelease/build candidate; final semantic version requires owner decision).
