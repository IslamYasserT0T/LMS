using Casbin;
using IY.Reporting.Models;
using IY.Reporting.Services;

namespace LMS.ReportingModule;

public sealed class LmsReportAccessPolicy : IReportAccessPolicy
{
    private const string Domain = "lms";
    private readonly IEnforcer _enforcer;

    public LmsReportAccessPolicy(IEnforcer enforcer) => _enforcer = enforcer;

    public Task<bool> IsAuthorizedAsync(IReportExecutionContext context, ReportAccessRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Guid.TryParse(context.SubjectId, out _)) return Task.FromResult(false);

        var resource = request.Operation switch
        {
            ReportOperation.SourceAccess => $"reporting:table:{request.TableName}",
            ReportOperation.Project or ReportOperation.Filter or ReportOperation.Sort or ReportOperation.Group or ReportOperation.Aggregate
                => $"reporting:column:{request.Column?.Name}",
            ReportOperation.Join => $"reporting:join:{request.Join?.TargetTable}",
            ReportOperation.Export => "reporting:export",
            _ => "reporting:unknown"
        };

        var action = request.Operation.ToString().ToLowerInvariant();
        return Task.FromResult(_enforcer.Enforce(context.SubjectId, resource, action, Domain));
    }
}
