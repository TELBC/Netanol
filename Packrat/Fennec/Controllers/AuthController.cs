using System.Diagnostics;
using System.Security.Claims;
using Fennec.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fennec.Controllers;

public record LoginRequest(string Username, string Password);

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILogger _log;
    private readonly SignInManager<NetanolUser> _signInManager;
    private readonly UserManager<NetanolUser> _userManager;

    public AuthController(SignInManager<NetanolUser> signInManager, ILogger log, UserManager<NetanolUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _log = log.ForContext<AuthController>();
    }

    /// <summary>
    ///     Sign in using the passed credentials.
    /// </summary>
    /// <remarks>
    ///     Try to log in using the username and password in the <see cref="LoginRequest" />. Once completed the backend
    ///     will tell the browser, via HTTP headers, to set a cookie which will be transmitted on all subsequent
    ///     requests. This means the frontend does not have to handle tokens.
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The application is now logged in.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized,
        "The login attempt failed, reference the message for more information.")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        _log.Debug("Attempting login for {Username}", request.Username);
        var user = await _userManager.FindByNameAsync(request.Username);

        if (user == null)
        {
            _log.Debug("Login attempt for {Username} failed... User doesn't exist", request.Username);
            return Unauthorized("Invalid credentials");
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, true);
        if (result.IsNotAllowed)
        {
            _log.Debug("Login attempt for {Username} failed... Not allowed", request.Username);
            return Unauthorized("Invalid credentials");
        }

        if (result.IsLockedOut)
        {
            _log.Debug("Login attempt for {Username} failed... Locked out", request.Username);
            return Unauthorized("Too many failed login attempts, retry later");
        }

        if (!result.Succeeded)
        {
            _log.Debug("Login attempt for {Username} failed... Invalid password", request.Username);
            return Unauthorized("Invalid credentials");
        }

        _log.Debug("Login attempt for {Username} succeeded -> Generating JWT token", request.Username);
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.Role, "admin")
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return NoContent();
    }

    /// <summary>
    ///     Check if the application is still authenticated.
    /// </summary>
    /// <remarks>
    ///     This is intended to be used to check if the frontend is still logged in via the cookie.
    /// </remarks>
    /// <returns></returns>
    [Authorize]
    [SwaggerResponse(StatusCodes.Status200OK, "The request is authenticated.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request is not authenticated.")]
    [HttpGet("status")]
    public IActionResult Status()
    {
        return NoContent();
    }

    /// <summary>
    ///     Logout and remove the authentication cookie from storage.
    /// </summary>
    /// <remarks>
    ///     TODO: currently broken
    /// </remarks>
    /// <returns></returns>
    [HttpGet("logout")]
    [SwaggerResponse(StatusCodes.Status204NoContent,
        "The request completed and the application is no longer authenticated.")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }
}
