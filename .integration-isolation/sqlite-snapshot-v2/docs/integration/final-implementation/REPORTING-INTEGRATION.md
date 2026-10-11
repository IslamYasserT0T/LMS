# Reporting Integration

LMS Reporting module now registers host-owned adapters over IY.Reporting 1.0.1:

* `LmsReportExecutionContext`: authenticated Guid principal only; anonymous/malformed identity fails closed.
* `LmsReportAccessPolicy`: Casbin `lms` domain bridge; default deny.
* `LmsReportDataScopeProvider`: mandatory `Samples.CreatedBy == authenticated subject` filter.
* `LmsReportExportPolicy`: Casbin export permission plus 1,000-row maximum.
* `LmsReportingSchema`: allow-listed existing Sample columns only; no identity tables or joins.
* SqlKata PostgreSQL executor: secured pipeline only, parameterized package executor, 30-second Npgsql command timeout.

Saved definition persistence, database migration and end-to-end report execution remain incomplete/unverified pending disposable PostgreSQL.
