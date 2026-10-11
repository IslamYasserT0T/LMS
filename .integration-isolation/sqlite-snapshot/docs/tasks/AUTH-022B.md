# AUTH-022B: Package IdentityModule for Dynamic Deployment
Status: COMPLETE. Gate: L2.

## Objective and Why
Make IdentityModule discoverable through the existing manifest/plugins layout with its complete published dependency closure.

## Projects and Files
`ManifestPlugins/IdentityModuleManifest.json`; `modules/IdentityModule/PluginManifest.targets`; `src/LMS.Host/LMS.Host.csproj`.

## Prerequisites
AUTH-022A complete.

## Implementation
1. Created `ManifestPlugins/IdentityModuleManifest.json` pointing to `modules/IdentityModule/LMS.IdentityModule/bin/Debug/net9.0/plugins`.
2. Created `modules/IdentityModule/PluginManifest.targets` for MSBuild publish integration and imported it in `src/LMS.Host/LMS.Host.csproj`.
3. Unified `Microsoft.IdentityModel.Tokens` and `Microsoft.IdentityModel.JsonWebTokens` to 8.4.0 in `Shared.Application.csproj` to align with OpenIddict dependencies and resolve dynamic assembly loading conflicts under .NET 9.
4. Published complete closure of `LMS.IdentityModule` including `IY.Auth.*`, `OpenIddict.*`, and `Npgsql.*`.

## Constraints and Security
- Host remains package-reference-free.
- No development certificates or secrets shipped in plugin output.

## Verification and Behavioral Tests
- Published closure inspected in `modules/IdentityModule/LMS.IdentityModule/bin/Debug/net9.0/plugins/`: contains all `IY.Auth.*`, `OpenIddict.*`, and `Npgsql.*` dependencies.
- Host startup verified with `dotnet run --project src/LMS.Host/LMS.Host.csproj --no-build`:
  `Loaded module from D:\LMS\src\LMS.Host\bin\Debug\net9.0\plugins\LMS.IdentityModule\LMS.IdentityModule.dll`
- All sample modules and IdentityModule discovered and registered.
- Final DI container built successfully.

## Deliverables and Completion Evidence
- `ManifestPlugins/IdentityModuleManifest.json` and `PluginManifest.targets` created.
- Host discovery verified in runtime startup log.
- Gate L2 evaluated.