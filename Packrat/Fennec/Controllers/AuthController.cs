using Fennec.Database;
using Fennec.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fennec.Controllers;

public record LoginRequest(string Username, string Password);

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly SignInManager<NetanolUser> _signInManager;
    private readonly UserManager<NetanolUser> _userManager;
    private readonly ILogger _log;

    public AuthController(SignInManager<NetanolUser> signInManager, ILogger log, IJwtService jwtService, UserManager<NetanolUser> userManager)
    {
        _signInManager = signInManager;
        _jwtService = jwtService;
        _userManager = userManager;
        _log = log.ForContext<AuthController>();
    }

    [HttpPost("login")]
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
        var token = _jwtService.GenerateJwtToken(user, roles);
        return Ok(new { token });
    }
}