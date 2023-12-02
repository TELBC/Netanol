namespace Fennec.Options;

using System.ComponentModel.DataAnnotations;

// TODO: rework configuration to use validation
public class SecurityOptions
{
    public bool Enabled { get; set; }
    
    [Required] 
    public AccessOptions Access { get; set; }

    public class AccessOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
