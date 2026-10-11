# IY.Auth Readiness

## Verdict: BLOCKED for LMS adoption

`D:\IYAUTH\IY.AUTH` declares package version 0.2.0 and net9.0 in `Directory.Build.props`; its reusable packages and protocol adapter are present. The source locates OpenIddict inside `IY.Auth.OpenIddict`, while LMS references the ASP.NET Core/contracts boundary. Stores, client grants (authorization code, refresh token, client credentials and PKCE), cookie/bearer tests, and claims-protection tests exist.

This is not a current LMS adoption proof. LMS pins 0.1.2 in `modules/IdentityModule/LMS.IdentityModule/LMS.IdentityModule.csproj`, so its compiled host cannot claim use of the approved 0.2.0 baseline. The clean-consumer/sample host and associated tests currently target net10.0, so they do not constitute fresh net9 host-consumption evidence. No current full test run was performed in the Auth repository.

## Verified by inspection

* Guid-oriented Identity contracts/stores and OpenIddict adapter separation are present.
* ASP.NET Core adapter exposes endpoint registration; LMS calls `UseOpenIddict` and registers endpoint mapping actions.
* LMS account controller uses the `IWebAuthenticationService` contract for login/logout and restricts return URLs to local URLs.
* LMS API authorization explicitly selects `OpenIddict.Validation.AspNetCore` in `PermissionsController`.

## Pending / not proven

* Current 0.2.0 package build, hashes, NuGet dependency graph and net9 consumer test.
* LMS runtime migration/identity store startup against an isolated PostgreSQL database.
* Browser SSR/cookie/anonymous/unauthorized behavior and Blazor authentication state.
* Production signing/encryption certificate configuration and secret provisioning.
* LMS principal-to-Casbin policy mapping and reporting-context construction.

The existence of source tests is not reported as a pass without execution evidence.
