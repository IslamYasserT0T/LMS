# LMS-D11 — Existing Tests and Runtime Validation

**Workstream:** LMS-D11  
**Status:** Source-inspected only. Tests NOT executed (database connectivity required for integration tests; no destructive risk for unit tests).

---

## 1. Test Projects Inventory

| Project | Path | Framework |
|---|---|---|
| `SampleModule.Test` | `modules/SampleModule/SampleModule.Test/` | xUnit |
| `LMS.ReportingModule.Test` | `modules/ReportingModule/LMS.ReportingModule.Test/` | xUnit |

---

## 2. LMS.ReportingModule.Test — Full Coverage Analysis

### 2.1 ModuleFolderCopierTests

**File:** `LMS.ReportingModule.Test/ModuleFolderCopierTests.cs`

| Test | Behavior Tested | Classification |
|---|---|---|
| `CopyDirectory_PreservesNestedHierarchy_WithoutFlattening` | Directory copy preserves subdirectory structure | Unit (filesystem, temp dir) |
| `CopyDirectory_PreventsPathTraversal` | Normal copy succeeds safely | Unit (filesystem, temp dir) |
| `CopyFoldersFromManifest_InReleaseMode_RejectsDebugPath` | Release mode rejects Debug plugin path | Unit (filesystem, temp dir) |
| `CopyFoldersFromManifest_InReleaseMode_ThrowsIfSourceFolderDoesNotExist` | Missing source throws `DirectoryNotFoundException` | Unit (filesystem, temp dir) |

**Assessment:** All 4 tests are pure unit tests using temp directories. Safe to run without database. Test the deployment infrastructure thoroughly.

**Gap:** No test for debug-mode behavior (CopyFolder succeeds when source exists). No test for concurrent copy.

---

### 2.2 PluginDeploymentRegressionTests

**File:** `LMS.ReportingModule.Test/PluginDeploymentRegressionTests.cs`

| Test | Behavior Tested | Classification | Requires Built DLL |
|---|---|---|---|
| `ReleaseManifestResolution_ResolvesReleasePath_AndContainsNoDebugPath` | Manifest merge produces Release paths | Unit + filesystem | No (ManifestPlugins/) |
| `DebugManifestResolution_ResolvesDebugPath` | Manifest merge produces Debug paths | Unit + filesystem | No |
| `DeployedReleaseReportingModule_Metadata_DoesNotContainProofTypes` | Release DLL lacks Proof types | Assembly reflection | Yes (Release build) |
| `DeployedDependencies_DIAbstractions_ResolvesNet9Compatible_Not10` | `DI.Abstractions` is 9.x in deps.json | Text search on deps.json | Yes (Release build) |
| `DeployedDependencies_IYReporting_IsVersion101_No100Remains` | IY.Reporting is 1.0.1 in deps.json | Text search on deps.json | Yes (Release build) |

**Assessment:** First 2 tests are safe to run. Tests 3-5 require a Release build of `LMS.ReportingModule` at a specific path. Not executed per mission scope.

**Gap:** No tests for manifest merge idempotency. No tests for concurrent manifest writes.

---

### 2.3 SecurityIsolationTests

**File:** `LMS.ReportingModule.Test/SecurityIsolationTests.cs`

| Test | Behavior Tested | Requires Release DLL |
|---|---|---|
| `ReleaseCompilation_Metadata_ContainsNoProofTypesOrRoute` | Release lacks Proof types | Yes |
| `ReleaseCompilation_Metadata_ContainsAllProductionReportingPages` | Release has production pages | Yes |
| `ReleaseModule_RegisterServices_DoesNotRegisterProofServices_EvenWithExplicitOptIn` | Release module.RegisterServices ignores proof opt-in | Yes |
| `ReleaseModule_ProductionReporting_RemainsFailClosed_WithoutAdapters` | DI throws for IReportExecutionContext without real adapters | Yes |
| `DebugModule_WhenProofDisabled_DoesNotRegisterProofServices` | Debug + disabled flag → no proof services | No (Debug-only compilation) |
| `DebugModule_WhenProofExplicitlyEnabled_RegistersSyntheticHarness` | Debug + enabled flag → proof services registered | No |
| `DebugModule_ProofIdentity_IsSyntheticAndNonPrivileged` | Proof subject ID and claims are non-privileged | No |
| `DebugModule_ProofAccessPolicy_IsFailClosedDenyByDefault` | ProofReportAccessPolicy returns false | No |
| `DebugModule_ProofQueryExecutor_IsSyntheticInMemoryOnly` | ProofQueryExecutor has no real DB connection | No |
| `DebugModule_RealRegisterServices_EnforcesHostEnvironmentAndOptIn` (Theory, 4 cases) | Production env rejects proof even with opt-in | No |
| `DebugModule_RealRegisterServices_FromServicesCollection_RejectsProduction` | Services-registered prod env blocks proof | No |

**Assessment:** Debug-mode tests (marked `#if DEBUG`) are safe to run without infrastructure. Release-mode tests require Release build artifacts.

---

### 2.4 ComponentVerificationTests

**File:** `LMS.ReportingModule.Test/ComponentVerificationTests.cs`

| Test | Behavior Tested |
|---|---|
| `VerifyViewerAndDesignerSignatures` | IY.Reporting.Blazor component property contracts (Schema, Capabilities, Parameter attributes) |
| `VerifyWrapperComponents_RenamedAndRouteBound` | LMS.ReportingModule pages exist with correct `@page` routes |

**Assessment:** Reflection-based, no database. Safe to run. Tests verify the Blazor component API surface.

---

## 3. SampleModule.Test — Coverage Analysis

**Files inspected:** `SampleModuleContextGenerator.cs`, `TestSetup/Data/`

The test setup contains a context generator for integration tests. The `TestService/` subdirectory was not listed (empty or not reachable). No test files were found beyond the setup scaffolding in the inspection.

**Classification:** Integration tests requiring database setup (SQLite or PostgreSQL). Not safe to run without a database.

**Gap:** No unit tests for SampleModule application services. No unit tests for SampleModule repositories.

---

## 4. Missing Test Coverage (Gaps)

| Behavior | Gap |
|---|---|
| `ModuleManager.LoadModule()` | No unit test for plugin loading logic |
| `ModuleManager.RegisterModuleServices()` | No unit test |
| `IModuleManager.InitializeRuntimeAsync()` | No unit test for setting/nav providers |
| Module startup failure handling | No test verifying that failed module is skipped gracefully |
| Satellite assembly resolution | No test |
| Companion Blazor assembly discovery | No test |
| `IModule.IsEnabled` skip behavior | No test (behavior doesn't exist in code either) |
| Module ordering | No test |
| Dependency injection registration validation | No test |
| `Bootstrapper.PreInitialize` solution-root walk | No test |
| Cross-module service resolution | No test |
| Migration ordering | No test |
| Concurrent module loading | No test |
| AuthorizationModule DI | No test |
| SettingModule runtime hook | No test |

---

## 5. Safe Tests to Run (Not Run Per Mission Scope)

The following tests could be run safely (no database, no side effects):
- `ModuleFolderCopierTests` (4 tests) — temp directory only
- `PluginDeploymentRegressionTests` first 2 tests — filesystem only
- `SecurityIsolationTests` debug-mode tests (when compiled as Debug)
- `ComponentVerificationTests` (2 tests) — reflection only

**Reason not executed:** Mission scope is discovery only. Safe tests were inspected for coverage analysis purposes.

---

## 6. Test Commands (Not Executed)

```powershell
# Would need to be run from solution root
dotnet test modules/ReportingModule/LMS.ReportingModule.Test/LMS.ReportingModule.Test.csproj --filter "ModuleFolderCopierTests|ComponentVerificationTests"
```

**Not executed per mission constraints.** No destructive risk was identified for the listed safe tests, but the mission is discovery-only.
