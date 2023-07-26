using Microsoft.EntityFrameworkCore;
using Tapas.Models;

namespace Tapas.Database;

public class TapasContext : DbContext
{
    public DbSet<SingleTrace> SingleTraces => Set<SingleTrace>();

    public TapasContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Tapas");
    }
}