# Implementation Changelog

2026-10-11 changes in LMS only:

* Pinned `IY.Auth.AspNetCore` and `IY.Auth.Contracts` to 0.2.0.
* Added LMS Reporting adapters: authenticated Guid execution context, Casbin policy bridge, `CreatedBy` mandatory Sample scope, explicit export policy, allow-listed Sample schema, PostgreSQL SqlKata registration.
* Added Blazor cascading authentication-state registration.
* Updated Casbin permission controller to accept `sub` or `ClaimTypes.NameIdentifier` Guid subject.
* Updated Reporting isolation tests for real production adapters and added SqlKata test runtime dependency.

No IY.Auth or IY.Reporting source changed. No package was repacked or overwritten.
