# ADR-012: Export Integration

## Context
IY.Reporting provides CSV, HTML, Excel, and PDF exporters. The default package export policy fails closed.

## Decision
LMS must explicitly register an `IReportExportPolicy` that checks Casbin (`Reporting.Export`). CSV and HTML will be enabled. EPPlus (Excel) and iText (PDF) licensing remains a HUMAN / LEGAL decision and will not be enabled silently.
