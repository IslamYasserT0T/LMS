# AUTH-021B: Establish Host Authentication Middleware Order
Status: COMPLETE. Gate: L1 (In Progress).

## Objective and Why
Make Host execute authentication before authorization. `Bootstrapper.ConfigureApplication` currently executes only `UseAuthorization()`.

## Projects and Files
`src/LMS.Host`; `Services/Bootstrapper.cs`, `PermissionsController.cs`.

## Prerequisites
AUTH-021A complete; no IY.Auth package registration yet.

## Implementation
Added `_builder.Services.AddAuthentication()` in `Bootstrapper.PreInitialize()` and standard `app.UseAuthentication()` immediately preceding `app.UseAuthorization()` in `Bootstrapper.ConfigureApplication()`. Preserved routing, antiforgery, HTTPS, static files, controllers, and Razor component mappings. Added `[Authorize]` protected test endpoint `api/Permissions/test-protected` to verify authorization enforcement.

## Constraints and Security
Host executes middleware but does not own IY.Auth dependencies. Middleware supports later cookie and bearer schemes without bespoke token code.

## Verification and Behavioral Tests
- Build succeeds with 0 errors.
- Verified pipeline sequence in `Bootstrapper.cs`: `UseRouting()` -> `UseHttpsRedirection()` -> `UseStaticFiles()` -> `MapStaticAssets()` -> `UseAuthentication()` -> `UseAuthorization()` -> `UseAntiforgery()` -> `MapControllers()` -> `MapRazorComponents()`.
- Verified zero IY.Auth dependencies in `LMS.Host.csproj`.
- Unit test suite (`dotnet test D:\LMS\LMS.sln`) passed: 1 passed, 0 failed.

## Deliverables and Completion Evidence
- Pipeline change committed in `src/LMS.Host/Services/Bootstrapper.cs`.
- Verification endpoint added in `src/LMS.Host/PermissionsController.cs`.
- Updated execution state unblocking AUTH-021C.