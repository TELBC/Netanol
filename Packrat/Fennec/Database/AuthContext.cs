using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database;

public class NetanolRole : IdentityRole { }

public class NetanolUser : IdentityUser { }

public class AuthContext : IdentityDbContext<NetanolUser, NetanolRole, string>
{
    public AuthContext(DbContextOptions<AuthContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("auth");
        base.OnModelCreating(builder);
    }
}