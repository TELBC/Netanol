using Fennec.Database.Domain.Technical;

namespace Fennec.Database.Domain.Layout;

/// <summary>
/// A <see cref="GraphNode"/> that represents a <see cref="NetworkHost"/> on the
/// graph.
/// </summary>
public class HostNode : GraphNode
{
#pragma warning disable CS8618
    public HostNode(long layoutResetId, string displayName, long networkHostId, long? islandGroup) : base(
        layoutResetId, displayName)
#pragma warning restore CS8618
    {
        NetworkHostId = networkHostId;
        IslandGroup = islandGroup;
    }

    public HostNode(Layout layout, string displayName, NetworkHost networkHost, long? islandGroup) :
        base(layout, displayName)
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
    /// The *IslandGroup* this <see cref="HostNode"/> is a member of. If not
    /// set it is not part of a *IslandGroup*.
    /// </summary>
    public long? IslandGroup { get; set; }
}