# G0-01 Decision

## G0 BLOCKED — PACKAGE PROVENANCE

The IY.Auth recovery manifest records an approved UI package SHA-256 of `497FCA2B4D89C592199A09AAA52D7CFA1C1160B13F518A9F684BCE2B6AB4F16C`; the package currently located in the recorded audit cache hashes to `214124F94A4B03A525168E42F9E6516DE4EBF26D46320D6B6005313DF64BB960`. Six sibling packages match. This single mismatch breaks the seven-package 0.2.0 artifact set's recovery provenance.

Required owner action: locate the original approved IY.Auth.UI 0.2.0 artifact or provide an authoritative signed/hash manifest explaining the replacement. Do not overwrite, repack, or relabel the current file. Then attach the resolved manifest to a committed/release record.

Secondary constraints: IY.Reporting current source has no committed source-to-1.0.1-artifact chain; R3 is an uncommitted LMS working-tree change; the live host lock requires the isolated snapshot strategy. These do not change the primary G0 decision.

No LMS-REPORTING-003 work was started.
