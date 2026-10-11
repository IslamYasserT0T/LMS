using IY.Reporting.Models;
using IY.Reporting.Services;

namespace LMS.ReportingModule;

public sealed class LmsReportDataScopeProvider : IReportDataScopeProvider
{
    public Task<IReadOnlyList<ReportFilterRequest>> GetMandatoryFiltersAsync(string tableName, IReportExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Guid.TryParse(context.SubjectId, out _))
            throw new UnauthorizedAccessException("Reporting scope requires a Guid LMS subject.");

        if (!string.Equals(tableName, LmsReportingSchema.SamplesTable, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Reporting table has no LMS mandatory scope.");

        return Task.FromResult<IReadOnlyList<ReportFilterRequest>>(
            new[] { new ReportFilterRequest(LmsReportingSchema.CreatedByColumn, "eq", context.SubjectId) });
    }
}
