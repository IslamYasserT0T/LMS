using Casbin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.Host;

[Route("api/[controller]")]
[ApiController]
public class PermissionsController : ControllerBase
{
    private readonly IEnforcer _enforcer;

    public PermissionsController(IEnforcer enforcer)
    {
       _enforcer = enforcer;
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public IActionResult AddPremission()
    {
        if (!TryGetHumanSubject(out var subject))
            return Forbid();

        _enforcer.AddPermissionForUser(subject, "domain1", "read");
        return Ok();
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public IActionResult GetPermissions()
    {
        if (!TryGetHumanSubject(out var subject))
            return Forbid();

        var permissions = _enforcer.GetPermissionsForUser(subject);
        return Ok(permissions);
    }

    [HttpGet("CheckPermission")]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public IActionResult CheckPermission()
    {
        if (!TryGetHumanSubject(out var subject))
            return Forbid();

        var hasPermission = _enforcer.HasPermissionForUser(subject, "domain1", "read");
        return Ok(hasPermission);
    }

    [HttpGet("cookie-protected")]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public IActionResult CookieProtected()
    {
        return Ok();
    }

    [HttpGet("test-protected")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public IActionResult TestProtected()
    {
        return Ok("authenticated");
    }

    private bool TryGetHumanSubject(out string subject)
    {
        subject = string.Empty;
        var value = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
            return false;

        subject = userId.ToString("D");
        return true;
    }
}
