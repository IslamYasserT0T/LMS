#if DEBUG
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using IY.Reporting.Services;

namespace LMS.ReportingModule;

public class ProofReportDataScopeProvider : IReportDataScopeProvider
{
    public Task<IReadOnlyList<IY.Reporting.Models.ReportFilterRequest>> GetMandatoryFiltersAsync(string reportId, IY.Reporting.Models.IReportExecutionContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<IY.Reporting.Models.ReportFilterRequest>>(new List<IY.Reporting.Models.ReportFilterRequest>());
    }
}

public class ProofReportAccessPolicy : IReportAccessPolicy
{
    public Task<bool> IsAuthorizedAsync(IY.Reporting.Models.IReportExecutionContext context, IY.Reporting.Services.ReportAccessRequest request, CancellationToken cancellationToken)
    {
        // Fail-closed access policy
        return Task.FromResult(false);
    }
}

public class ProofQueryExecutor : IY.Reporting.Provider.QueryExecutorBase
{
    protected override Task<IY.Reporting.Services.ReportResult> ExecuteCoreAsync(IY.Reporting.Models.ResolvedReportPlan plan, IY.Reporting.Models.IReportExecutionContext context, CancellationToken cancellationToken)
    {
        // Return synthetic in-memory results only with no external database dependency
        var result = new IY.Reporting.Services.ReportResult(
            System.Array.Empty<IY.Reporting.Models.ResolvedColumn>(),
            System.Array.Empty<System.Collections.Generic.IReadOnlyDictionary<string, object?>>()
        );
        return Task.FromResult(result);
    }
}
#endif
