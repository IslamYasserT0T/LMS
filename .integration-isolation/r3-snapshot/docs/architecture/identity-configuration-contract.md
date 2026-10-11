# LMS Identity Configuration Contract & Deployment Specification

## 1. Overview
This document defines the configuration contract for the LMS + IY.Auth identity integration as required by AUTH-021C.

## 2. Configuration Section: `IYAuth`

| Key | Type | Default | Description | Environment Variable Override |
|---|---|---|---|---|
| `Issuer` | string | `https://localhost:5001` | Token authority issuer URL | `IYAUTH__ISSUER` |
| `DefaultSchema` | string | `identity` | Dedicated PostgreSQL schema for Identity tables | `IYAUTH__DEFAULTSCHEMA` |
| `RequireConfirmedEmail` | bool | `false` | Whether confirmed email is required for login | `IYAUTH__REQUIRECONFIRMEDEMAIL` |
| `RefreshTokenEnabled` | bool | `true` | Enables OAuth refresh token flow | `IYAUTH__REFRESHTOKENENABLED` |
| `ClientCredentialsEnabled` | bool | `true` | Enables OAuth client credentials flow | `IYAUTH__CLIENTCREDENTIALSENABLED` |
| `PasswordGrantEnabled` | bool | `false` | ROPC grant (disabled by default for security) | `IYAUTH__PASSWORDGRANTENABLED` |
| `LoginPath` | string | `/Account/Login` | Web UI cookie challenge/login path | `IYAUTH__LOGINPATH` |
| `LogoutPath` | string | `/Account/Logout` | Web UI cookie signout path | `IYAUTH__LOGOUTPATH` |
| `AccessDeniedPath` | string | `/Account/AccessDenied` | Forbidden access redirect path | `IYAUTH__ACCESSDENIEDPATH` |

## 3. Credentials Configuration Section: `IYAuth:Credentials`

| Key | Type | Default | Description | Production Requirement |
|---|---|---|---|---|
| `UseDevelopmentCertificates` | bool | `false` | Self-signed development certificates for OpenIddict | **MUST be `false` in Production**. Only allowed `true` in Development. |
| `SigningCertificatePfxPath` | string | `""` | File path to signing certificate PFX | Required in production unless store thumbprint provided. |
| `SigningCertificatePfxPassword` | string | `""` | Password for signing certificate PFX | Injected via secret manager or env variable. |
| `EncryptionCertificatePfxPath` | string | `""` | File path to encryption certificate PFX | Required in production unless store thumbprint provided. |
| `EncryptionCertificatePfxPassword` | string | `""` | Password for encryption certificate PFX | Injected via secret manager or env variable. |
| `SigningCertificateThumbprint` | string | `""` | Thumbprint for certificate store lookup | Alternative to PFX path. |
| `EncryptionCertificateThumbprint` | string | `""` | Thumbprint for certificate store lookup | Alternative to PFX path. |
| `CertificateStoreLocation` | string | `CurrentUser` | X509Store location | `CurrentUser` or `LocalMachine`. |
| `CertificateStoreName` | string | `My` | X509Store name | `My`, `Root`, etc. |

## 4. Production Fail-Closed Rule
In Production (`ASPNETCORE_ENVIRONMENT=Production`):
1. `UseDevelopmentCertificates` defaults to `false`. If set to `true` or missing valid certificates, OpenIddict fails closed at startup to prevent running insecure tokens in production.
2. Valid signing and encryption certificates must be provided via either PFX file path + password or X509 store thumbprint.
3. No secrets are stored in source-controlled configuration files.

## 5. Development Mode
In Development (`ASPNETCORE_ENVIRONMENT=Development`):
- `appsettings.Development.json` enables `"UseDevelopmentCertificates": true`.
- Ephemeral development signing and encryption keys are generated in-memory.