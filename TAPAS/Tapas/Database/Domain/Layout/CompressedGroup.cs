namespace Tapas.Models;

/// <summary>
/// Represents a compressed group of <see cref="NetworkDevice"/>s which should
/// be represented in the graph as one node. This class matches one
/// <see cref="NetworkDevice"/> to one <see cref="GraphNode"/> and thus represents a m:n table.
/// </summary>
public class CompressedGroup
{
    public long Id { get; set; }

    /// <summary>
    /// The <see cref="GraphNode"/> the <see cref="NetworkDevice"/> belongs into.
    /// </summary>
    public GraphNode GraphNode { get; set; }
    public long GraphNodeId { get; set; }
    
    /// <summary>
    /// The <see cref="NetworkDevice"/> which belongs into the <see cref="GraphNode"/>.
    /// </summary>
    public NetworkDevice NetworkDevice { get; set; }
    public long NetworkDeviceId { get; set; }

#pragma warning disable CS8618
    public CompressedGroup(long graphNodeId, long networkDeviceId)
#pragma warning restore CS8618
    {
        GraphNodeId = graphNodeId;
        NetworkDeviceId = networkDeviceId;
    }

    public CompressedGroup(GraphNode graphNode, NetworkDevice networkDevice)
    {
        GraphNode = graphNode;
        NetworkDevice = networkDevice;
    }
    
#pragma warning disable CS8618
    public CompressedGroup() { }
#pragma warning restore CS8618
}