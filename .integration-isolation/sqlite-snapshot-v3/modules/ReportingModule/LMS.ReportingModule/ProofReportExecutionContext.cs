#if DEBUG
using System.Collections.Generic;
using IY.Reporting.Models;

namespace LMS.ReportingModule;

public class ProofReportExecutionContext : IReportExecutionContext
{
    public string SubjectId => "synthetic-proof-runner-unprivileged";
    public IReadOnlyDictionary<string, string> Claims => new Dictionary<string, string>
    {
        { "Scope", "integration-proof-only" },
        { "Privilege", "None" },
        { "Role", "NonPrivilegedProofRunner" }
    };
    public string CorrelationId => "proof-run";
    public string PartitionKey => "none";
}
#endif
