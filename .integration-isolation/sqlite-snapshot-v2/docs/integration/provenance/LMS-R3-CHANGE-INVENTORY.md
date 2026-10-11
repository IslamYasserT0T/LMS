# LMS R3 Change Inventory

All entries are uncommitted or untracked unless noted. SHA-256 values freeze the observed source, not a release.

| Area | Files / SHA-256 | Classification |
|---|---|---|
| Proof isolation | `Module.cs` `79C350EE…C7C65`; `ProofProviders.cs` `2F4A3201…F1DC58`; `ProofReportExecutionContext.cs` `5A2C9509…19284B` | R3 security change: Debug-only proof services require opt-in and non-Production environment; proof identity/access/executor are synthetic/fail-closed |
| Manifests | Identity `1D58F66B…DC597E`; Sample `29FA8267…97D8D`; Reporting `90014BF9…B3CF3` | R3 deployment change: active plugin copy folders use `{Configuration}` |
| Copier/manifest runtime | `ModuleFolderCopier.cs` `4CF4FBF3…2D30A`; `ModuleManifestsManager.cs` `D5886C50…2A81B`; `Bootstrapper.cs` `D87D463A…23D18` | R3 deployment/static assets/localization support; configuration substitution, Release Debug-path rejection, hierarchy-preserving copies and plugin static-file serving |
| Module package/UI | csproj `A159AD2D…8AA2E`; `IntegrationProof.razor` `FD91E36F…57939`; `PluginManifest.targets` `14BBD01E…947D1` | R3 test/proof deployment infrastructure |
| Tests | Security `DFEA25C5…D8BB96`; Copier `E5B8EE80…1B67D5`; Deployment `2D0CA16A…CF9692` | R3 verification infrastructure, including actual Production-environment registration assertion |

Related LMS tracked edits outside this inventory include Identity integration, module manager/contracts, host project/configuration, permissions, solution and NuGet configuration. They are preserved and intentionally not attributed to R3 without a dedicated author-approved change record.

R3 localization result is **unverified**: copying satellite assemblies/assets is present in source/test infrastructure, but current Arabic browser/circuit behavior has not been rerun.
