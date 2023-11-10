namespace Fennec.Options;

using System.ComponentModel.DataAnnotations;

// TODO: rework configuration to use validation
public class SecurityOptions
{
    [Required] 
    public AccessOptions Access { get; set; }

    [Required] 
    public JwtOptions Jwt { get; set; }

    public class AccessOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool HideWhenNoAuth { get; set; }
    }

    public class JwtOptions
    {
        public string Issuer { get; set; } = "netanol";
        public string Audience { get; set; } = "netanol";
        public string? Key { get; set; }
    }
}
