using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fennec.Database;
using Fennec.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fennec.Services;

/// <summary>
/// Creates JWT tokens.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Create a JWT token for the given <see cref="NetanolUser"/> with the roles as claims.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="roles"></param>
    /// <returns>The token as string</returns>
    public string GenerateJwtToken(NetanolUser user, IList<string> roles);
}

public class JwtService : IJwtService
{
    private readonly SecurityOptions _securityOptions;

    public JwtService(IOptions<SecurityOptions> securityOptions)
    {
        _securityOptions = securityOptions.Value;
    }

    public string GenerateJwtToken(NetanolUser user, IList<string> roles)
    {
        var key = Encoding.UTF8.GetBytes(_securityOptions.Jwt.Key);
        var issuer = _securityOptions.Jwt.Issuer;
        var audience = _securityOptions.Jwt.Audience;
        var expiry = TimeSpan.FromHours(12); // TODO: implement refresh tokens

        var claims = new List<Claim>
        {
            new("id", user.Id)
        };
            
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(expiry),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = issuer,
            Audience = audience,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);
        return token;
    }
}