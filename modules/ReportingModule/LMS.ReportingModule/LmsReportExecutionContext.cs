using System.Security.Claims;
using IY.Reporting.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace LMS.ReportingModule;

public sealed class LmsReportExecutionContext : IReportExecutionContext
{
    private readonly ClaimsPrincipal _principal;
    private readonly string _subjectId;

    public LmsReportExecutionContext(IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authenticationStateProvider)
    {
        _principal = httpContextAccessor.HttpContext?.User
            ?? authenticationStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult().User;

        if (_principal.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("Reporting requires an authenticated LMS principal.");

        var subject = _principal.FindFirstValue("sub") ?? _principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out var userId))
            throw new UnauthorizedAccessException("Reporting requires a Guid LMS subject.");

        _subjectId = userId.ToString("D");
    }

    public string SubjectId => _subjectId;
    public IReadOnlyDictionary<string, string> Claims => _principal.Claims
        .GroupBy(c => c.Type, StringComparer.Ordinal)
        .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.Ordinal);
    public string CorrelationId => _principal.FindFirstValue("traceparent") ?? Guid.NewGuid().ToString("N");
    public string PartitionKey => _subjectId;
}
