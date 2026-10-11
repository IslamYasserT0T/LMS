using IY.Reporting.Export;
using IY.Reporting.Models;
using IY.Reporting.Services;

namespace LMS.ReportingModule;

public sealed class LmsReportExportPolicy : IReportExportPolicy
{
    private const int MaxRows = 1_000;
    private readonly IReportAccessPolicy _accessPolicy;

    public LmsReportExportPolicy(IReportAccessPolicy accessPolicy) => _accessPolicy = accessPolicy;

    public async Task EnsureCanExportAsync(ResolvedReportPlan plan, IReportExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (!await _accessPolicy.IsAuthorizedAsync(context, ReportAccessRequest.ForExport(), cancellationToken))
            throw new ExportDeniedException("LMS Casbin policy denied report export.");
    }

    public Task<int> GetMaximumExportRowsAsync(ResolvedReportPlan plan, IReportExecutionContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(MaxRows);
}
