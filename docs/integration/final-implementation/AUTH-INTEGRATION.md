# Auth Integration

LMS now pins Auth 0.2.0 in `modules/IdentityModule/LMS.IdentityModule/LMS.IdentityModule.csproj`. Identity module remains on IY.Auth public ASP.NET Core/contracts APIs. Cookie `NameIdentifier` is accepted as Guid fallback beside `sub` for LMS Casbin subject conversion.

Fresh command: `dotnet test IY.AUTH.sln -c Release -v:q` exited 0. Results: 134 passed, 1 optional PostgreSQL lifecycle test skipped. Clean consumer historical evidence: 21 passed; it targets net10.0 and is not net9 LMS-host proof.

Runtime login, bearer, refresh/revocation and Blazor browser checks remain unverified because disposable PostgreSQL is unavailable.
