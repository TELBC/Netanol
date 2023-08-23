using Fennec.Database.Domain.Technical;

namespace Fennec.Database.Domain.Layout;

/// <summary>
/// A <see cref="GraphNode"/> that represents a <see cref="NetworkHost"/> on the
/// graph.
/// </summary>
public class HostNode : GraphNode
{
#pragma warning disable CS8618
    public HostNode(long layoutResetId, string displayName, long networkHostId, long? islandGroupId) : base(
        layoutResetId, displayName)
#pragma warning restore CS8618
    {
        NetworkHostId = networkHostId;
        IslandGroupId = islandGroupId;
    }

    public HostNode(LayoutPreset layoutPreset, string displayName, NetworkHost networkHost, IslandGroup? islandGroup) :
        base(layoutPreset, displayName)
    {
        NetworkHost = networkHost;
        IslandGroup = islandGroup;
    }

#pragma warning disable CS8618
    public HostNode()
    {
    }
#pragma warning restore CS8618
    /// <summary>
    /// The <see cref="NetworkHost"/> this <see cref="HostNode"/> represents
    /// in the graph.
    /// </summary>
    public NetworkHost NetworkHost { get; set; }

    public long NetworkHostId { get; set; }

    /// <summary>
    /// The <see cref="IslandGroup"/> this <see cref="HostNode"/> is a member of. If not
    /// set it is not part of a <see cref="IslandGroup"/>.
    /// </summary>
    public IslandGroup? IslandGroup { get; set; }

    public long? IslandGroupId { get; set; }
}