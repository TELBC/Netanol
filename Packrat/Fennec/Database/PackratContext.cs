using Fennec.Database.Domain.Layout;
using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database;

/// <summary>
/// The <see cref="DbContext"/> storing all data for TAPAS.
/// </summary>
public interface IPackratContext
{
    DbSet<Layout> Layouts { get; }
    DbSet<GraphNode> GraphNodes { get; }
    DbSet<HostNode> HostNodes { get; }
    DbSet<CompressedGroup> CompressedGroups { get; }

    DbSet<NetworkHost> NetworkHosts { get; }
    DbSet<SingleTrace> SingleTraces { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class PackratContext : DbContext, IPackratContext
{
    public PackratContext(DbContextOptions<PackratContext> options) : base(options)
    {
    }

    public PackratContext()
    {
    }

    public DbSet<Layout> Layouts => Set<Layout>();
    public DbSet<GraphNode> GraphNodes => Set<GraphNode>();
    public DbSet<HostNode> HostNodes => Set<HostNode>();
    public DbSet<CompressedGroup> CompressedGroups => Set<CompressedGroup>();

    public DbSet<NetworkHost> NetworkHosts => Set<NetworkHost>();
    public DbSet<SingleTrace> SingleTraces => Set<SingleTrace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("fennec");
    }
}