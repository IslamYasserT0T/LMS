using Casbin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult AddPremission()
    {
        _enforcer.AddPermissionForUser("alice", "domain1", "read");
        return Ok();
    }

    [HttpGet]
    public IActionResult GetPermissions()
    {
        var permissions = _enforcer.GetPermissionsForUser("alice");
        return Ok(permissions);
    }

    [HttpGet("CheckPermission")]
    public IActionResult CheckPermission()
    {
        var hasPermission = _enforcer.HasPermissionForUser("alice", "domain1", "read");
        return Ok(hasPermission);
    }

    [HttpGet("test-protected")]
    [Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
    public IActionResult TestProtected()
    {
        return Ok("authenticated");
    }
}