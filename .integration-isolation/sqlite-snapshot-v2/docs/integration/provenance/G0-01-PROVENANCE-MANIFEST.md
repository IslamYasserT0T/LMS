# G0-01 Provenance Manifest

Audit time: 2026-10-11. Scope: read-only inspection plus this documentation. No repository was reset, cleaned, stashed, rebased, committed, packed, or otherwise mutated.

| Repository | Branch / HEAD | Staged | Unstaged | Untracked | Package declaration / artifacts |
|---|---|---:|---:|---:|---|
| LMS (`D:\LMS`) | `feature/IY_package_integration` / `b3eaa543c087577f43b074fb03cbf1d5b5e4f6a1` | 0 | 19 tracked paths | Reporting module, ADR/review/readiness documents | IY.Auth 0.1.2 requested; Reporting 1.0.1 requested; local build/plugin outputs exist but are not provenance evidence |
| IY.Auth (`D:\IYAUTH\IY.AUTH`) | `feature/UI` / `64bb8f089d1b4fa6d0cf9e900c7e786f6f771a0e` | 0 | 0 | `docs/integration/INTEGRATION-RECOVERY-001.md` | seven 0.2.0 artifacts in `.audit-nuget-cache`; one recorded hash mismatch |
| IY.Reporting (`D:\IY.Reporting`) | `master` / `10fe9477c3fba04513ea75f982d7809e2ffba646` | 0 | 20 tracked paths | three reports, two reviews, `scripts/verify-packages.ps1` | seven 1.0.1 artifacts in `artifacts/audit-1.0.1`; dirty source is not asserted to equal them |

## Live lock

PID 26420 is running as `LMS.Host.exe` from `D:\LMS\src\LMS.Host\bin\Debug\net9.0\LMS.Host.exe` (started 2026-10-11 12:18:50). It was not terminated. The prior Debug Reporting test build reproduced output-copy failure because this process holds host assemblies.

## Current build/test outputs

* LMS Reporting has existing Debug/Release `bin/net9.0` module/plugin trees and Debug/Release test output trees. They are stale/ambient output, not a clean result for this task.
* IY.Auth has Release build output dated 2026-10-11; the recovery record reports prior `134 passed, 1 skipped` and `21 clean-consumer tests passed`. Those are historical results, not tests executed by G0-01.
* IY.Reporting `bin` output dates are primarily 2026-09-27/29; current dirty source was not rebuilt or packed.

## Reproducible state capture

```powershell
git -C D:\LMS status --short
git -C D:\IYAUTH\IY.AUTH status --short
git -C D:\IY.Reporting status --short
git -C D:\LMS diff --cached --name-status
git -C D:\LMS diff --name-status
Get-Process -Id 26420 | Select-Object Id,Path,StartTime
```

The governing decision is package provenance, not host-process interference: a recorded approved IY.Auth package hash does not match the current artifact.
