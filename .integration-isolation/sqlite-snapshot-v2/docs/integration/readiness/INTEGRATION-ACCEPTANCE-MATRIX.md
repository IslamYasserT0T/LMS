# Integration Acceptance Matrix

| Gate | Acceptance evidence | Current result |
|---|---|---|
| G0 | clean commit IDs, package hashes and source/artifact correspondence | FAIL / incomplete |
| G1 | net9 IY.Auth package build and isolated LMS consumer flow | NOT RUN |
| G2 | seven fresh 1.0.1 packs, dependency scan, Release and consumer tests | historical pass only; current UNVERIFIED |
| G3 | Release host starts with all manifests; no Debug plugin path; RCL assets resolve | source likely corrected; runtime NOT RUN |
| G4 | Guid identity, cookie/bearer, claims and Casbin authorization in LMS | NOT RUN |
| G5 | Production + proof opt-in has no proof DI; Arabic/RTL browser proof | source/test coverage present; executable proof NOT RUN |
| G6 | authenticated subject reaches Casbin policy, mandatory scope and approved executor only | PENDING; not implemented |
| G7 | disposable PostgreSQL end-to-end negative/positive security suite | NOT RUN |
| G8 | independent review reproduces all prior gates from clean revisions | NOT RUN |

Attempted test: `dotnet test modules/ReportingModule/LMS.ReportingModule.Test/LMS.ReportingModule.Test.csproj -c Debug --no-restore` failed during build because PID 26420 (`LMS.Host`) locked copied host assemblies. No tests were counted as passed in this audit.
