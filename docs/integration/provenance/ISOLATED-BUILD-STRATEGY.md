# Isolated Build Strategy

## Constraint

The live LMS Host locks its Debug output. Never build into `D:\LMS\src\LMS.Host\bin`, the current module `bin` trees, or their plugin destinations while it runs.

## Committed-source gates

Use the existing detached worktree at `C:\Users\Dell\.codex\worktrees\2cc5\LMS` for checks requiring only committed LMS source at `b3eaa543c087577f43b074fb03cbf1d5b5e4f6a1`. It does not contain uncommitted R3.

## R3 snapshot gate (required for R3)

Create only after G0 package provenance is repaired, at the disposable path:

`D:\LMS\.integration-isolation\g0-r3-snapshot-<UTC timestamp>`

The snapshot must copy tracked and untracked working files while excluding `.git`, `bin`, `obj`, `.vs`, existing `plugins`, and `.integration-isolation`. Record a SHA-256 manifest of every copied source file before build. This snapshot must never be synchronized back to `D:\LMS`.

Run from the snapshot with unique paths (replace `<run>`):

```powershell
dotnet restore LMS.sln --packages D:\LMS\.integration-isolation\nuget\<run>
dotnet build LMS.sln -c Debug --no-restore `
  -p:BaseOutputPath=D:\LMS\.integration-isolation\out\<run>\ `
  -p:BaseIntermediateOutputPath=D:\LMS\.integration-isolation\obj\<run>\
dotnet test modules\ReportingModule\LMS.ReportingModule.Test\LMS.ReportingModule.Test.csproj -c Debug --no-restore `
  -p:BaseOutputPath=D:\LMS\.integration-isolation\out\<run>\ `
  -p:BaseIntermediateOutputPath=D:\LMS\.integration-isolation\obj\<run>\
```

Before executing, verify `PluginManifest.targets` and every plugin-copy target resolves under the snapshot output root. Pass a dedicated plugin deployment root/configuration where available; fail the run if any target resolves to `D:\LMS\**\bin`, `AppContext.BaseDirectory\plugins` of the live host, or an existing shared plugin path. Runtime tests must use a unique isolated database and test deployment root.

No isolated snapshot or build was created by G0-01 because package provenance is blocked and a build would not resolve that defect.
