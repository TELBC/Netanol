using Fennec.Database.Domain.Technical;

namespace Fennec.Database.Domain.Layout;

/// <summary>
/// Represents a compressed group of <see cref="NetworkHost"/>s which should
/// be represented in the graph as one node. This class matches one
/// <see cref="NetworkHost"/> to one <see cref="GraphNode"/> and thus represents a m:n table.
/// </summary>
public class CompressedGroup
{
#pragma warning disable CS8618
    public CompressedGroup(long graphNodeId, long networkHostId)
#pragma warning restore CS8618
    {
        GraphNodeId = graphNodeId;
        NetworkHostId = networkHostId;
    }

    public CompressedGroup(GraphNode graphNode, NetworkHost networkHost)
    {
        GraphNode = graphNode;
        NetworkHost = networkHost;
    }

#pragma warning disable CS8618
    public CompressedGroup()
    {
    }
#pragma warning restore CS8618
    public long Id { get; set; }

    /// <summary>
    /// The <see cref="GraphNode"/> the <see cref="NetworkHost"/> belongs into.
    /// </summary>
    public GraphNode GraphNode { get; set; }

    public long GraphNodeId { get; set; }

    /// <summary>
    /// The <see cref="NetworkHost"/> which belongs into the <see cref="GraphNode"/>.
    /// </summary>
    public NetworkHost NetworkHost { get; set; }

    public long NetworkHostId { get; set; }
}