# LMS Identity Execution State

| Task | Status |
|---|---|
| AUTH-021A | COMPLETE |
| AUTH-021B | COMPLETE |
| AUTH-021C | COMPLETE |
| Gate L1 | PASS |
| AUTH-022A | COMPLETE |
| AUTH-022B | COMPLETE |
| Gate L2 | PASS |
| AUTH-023A | COMPLETE |
| AUTH-023B | COMPLETE |
| Gate L3 | PASS |
| AUTH-024A | REQUIRES REVIEW: frozen IY.Auth application-cookie ticket omits authoritative `sub`; a public claims contributor does not alter that ticket. |
| AUTH-025A | IN PROGRESS: login issues the real `Identity.Application` cookie; remaining behavioral matrix is not closed. |
| AUTH-025B | READY |
| AUTH-026A | BLOCKED: approved Casbin subject requires cookie `sub`, but frozen IY.Auth 0.1.2 exposes only NameIdentifier Guid in its application-cookie principal. |
| AUTH-026B | BLOCKED BY AUTH-026A |
| AUTH-027A | BLOCKED BY GATE L5 |

Update this document with task evidence only after the applicable gate is accepted.
