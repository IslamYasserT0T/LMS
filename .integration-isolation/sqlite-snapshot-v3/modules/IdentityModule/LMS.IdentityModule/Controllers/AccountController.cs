using IY.Auth.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace LMS.IdentityModule.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly IWebAuthenticationService _authService;

    public AccountController(IWebAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        var error = Request.Query["error"].ToString();
        string errorMsg = error == "locked" ? "Account locked." : error == "invalid" ? "Invalid credentials." : "";
        var html = $@"
            <html><body>
                <h3>Login</h3>
                <form method='post' action='/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}' style='display:flex;flex-direction:column;max-width:300px;gap:10px;'>
                    <div style='color:red'>{errorMsg}</div>
                    <label>Username: <input name='username' class='form-control' /></label>
                    <label>Password: <input name='password' type='password' class='form-control' /></label>
                    <button type='submit' class='btn btn-primary'>Login</button>
                </form>
            </body></html>
        ";
        return Content(html, "text/html");
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginPost([FromForm] string username, [FromForm] string password, [FromQuery] string? returnUrl = null)
    {
        var result = await _authService.SignInAsync(username, password);
        if (result.IsSuccess)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }
        
        var err = result.Error?.Code == AuthErrorCodes.UserLockedOut ? "locked" : "invalid";
        return Redirect($"/Account/Login?error={err}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync();
        return Redirect("/");
    }
}
