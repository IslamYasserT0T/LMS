# Cross-Repository Integration Status

Audit date: 2026-10-11. Scope was read-only except for these audit documents.

| Repository | Located at | Branch / HEAD | State | Verdict |
|---|---|---|---|---|
| LMS | `D:\LMS` | `feature/IY_package_integration` / `b3eaa543c087577f43b074fb03cbf1d5b5e4f6a1` | Dirty: tracked LMS host changes and untracked Reporting module/docs | BLOCKED |
| IY.Auth | `D:\IYAUTH\IY.AUTH` (not `D:\IY.Auth`) | `feature/UI` / `64bb8f089d1b4fa6d0cf9e900c7e786f6f771a0e` | Dirty: untracked `docs/integration/` only | BLOCKED |
| IY.Reporting | `D:\IY.Reporting` | `master` / `10fe9477c3fba04513ea75f982d7809e2ffba646` | Extensive dirty package/test/review changes | UNVERIFIED |

## Executive conclusion

Approved reusable artifacts remain plausible baselines: IY.Auth 0.2.0 and seven IY.Reporting 1.0.1 packages. Current source cannot be promoted from those baselines because LMS consumes IY.Auth 0.1.2, IY.Reporting source is dirty without a fresh artifact/provenance chain, and the LMS host has no production reporting adapters. The attempted current LMS test run was blocked by a pre-existing `LMS.Host` PID 26420 assembly lock; no process was stopped.

R3 source changes address the prior Reporting Gate L2 production-proof and Debug-manifest findings, but they are uncommitted and have not received fresh executable runtime verification. Arabic culture/resource resolution remains unverified in the current tree.

## Current package references

* LMS Identity: `IY.Auth.AspNetCore` 0.1.2 and `IY.Auth.Contracts` 0.1.2.
* LMS Reporting: `IY.Reporting`, `.AspNetCore`, and `.Blazor` 1.0.1.
* IY.Auth source version: 0.2.0; release package cache contains all seven 0.2.0 packages.
* IY.Reporting source declares seven 1.0.1 packages; `artifacts/audit-1.0.1` contains seven matching package files, but no fresh hash/build attestation was produced for the dirty source.

See the component reports, blocker register, acceptance matrix, and dependency graph in this directory.
