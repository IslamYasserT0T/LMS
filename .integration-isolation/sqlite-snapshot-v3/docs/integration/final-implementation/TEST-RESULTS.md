# Test Results

| Command | Result |
|---|---|
| `dotnet test D:\IYAUTH\IY.AUTH\IY.AUTH.sln -c Release -v:q` | Exit 0; 134 passed, 1 skipped |
| `dotnet test ...IY.Auth.OpenIddict.Tests.csproj -c Release --no-build --no-restore -v:q` | 29 passed |
| Snapshot Reporting module Debug build | Passed, 0 errors |
| Snapshot Reporting module Release build | Passed, 0 errors; existing warnings |
| Snapshot Reporting Debug tests | 22 passed, 0 failed |
| PostgreSQL runtime suite | Not run: Docker Desktop Linux engine unavailable; listener on 5432 has unknown/non-disposable provenance |
| Browser suite | Not run: no isolated Host/database runtime |
