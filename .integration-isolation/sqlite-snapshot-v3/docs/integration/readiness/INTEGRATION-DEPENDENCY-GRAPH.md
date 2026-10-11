# Integration Dependency Graph

```text
G0 provenance
 ├─> G1 IY.Auth 0.2.0 verification ─> G4 LMS identity adoption
 ├─> G2 IY.Reporting 1.0.1 verification ─┐
 └─> G3 LMS Release/R3 stabilization ────┼─> G5 Reporting Gate L2 closure
                                         │              │
G4 identity principal + Casbin ──────────┼─> G6 LMS-REPORTING-003 adapters
G2 reusable package boundary ────────────┘              │
G3 isolated host/deployment ────────────────────────────┤
                                                        v
                                             G7 integrated security/runtime tests
                                                        v
                                             G8 independent release audit
```

G1 and G2 can proceed in parallel after G0. G3 can also proceed in parallel but G5 requires G3 completion. G6 must not start until the user authorizes implementation and requires G4/G5 inputs.
