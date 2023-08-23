using Fennec.Database.Domain.Layout;
using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database;

/// <summary>
/// The <see cref="DbContext"/> storing all data for TAPAS.
/// </summary>
public interface ITapasContext
{
    DbSet<LayoutPreset> LayoutPresets { get; }
    DbSet<GraphNode> GraphNodes { get; }
    DbSet<HostNode> DeviceNodes { get; }
    DbSet<IslandGroup> IslandGroups { get; }
    DbSet<CompressedGroup> CompressedGroups { get; }

    DbSet<NetworkHost> NetworkHosts { get; }
    DbSet<SingleTrace> SingleTraces { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class TapasContext : DbContext, ITapasContext
{
    public TapasContext(DbContextOptions options) : base(options)
    {
    }

    public TapasContext()
    {
    }

    public DbSet<LayoutPreset> LayoutPresets => Set<LayoutPreset>();
    public DbSet<GraphNode> GraphNodes => Set<GraphNode>();
    public DbSet<HostNode> DeviceNodes => Set<HostNode>();
    public DbSet<IslandGroup> IslandGroups => Set<IslandGroup>();
    public DbSet<CompressedGroup> CompressedGroups => Set<CompressedGroup>();

    public DbSet<NetworkHost> NetworkHosts => Set<NetworkHost>();
    public DbSet<SingleTrace> SingleTraces => Set<SingleTrace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Fennec");
    }
}