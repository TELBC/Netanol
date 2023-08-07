using Microsoft.EntityFrameworkCore;
using Tapas.Models;

namespace Tapas.Database;

/// <summary>
/// The <see cref="DbContext"/> storing all data for TAPAS.
/// </summary>
public class TapasContext : DbContext
{
    public DbSet<LayoutPreset> LayoutPresets => Set<LayoutPreset>();
    public DbSet<GraphNode> GraphNodes => Set<GraphNode>();
    public DbSet<DeviceNode> DeviceNodes => Set<DeviceNode>();
    public DbSet<IslandGroup> IslandGroups => Set<IslandGroup>();
    public DbSet<CompressedGroup> CompressedGroups => Set<CompressedGroup>();

    public DbSet<NetworkHost> NetworkHosts => Set<NetworkHost>();
    public DbSet<NetworkDevice> NetworkDevices => Set<NetworkDevice>();
    public DbSet<SingleTrace> SingleTraces => Set<SingleTrace>();

    public TapasContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Tapas");
    }
}