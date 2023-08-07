namespace Tapas.Models;

/// <summary>
/// A <see cref="GraphNode"/> that represents a <see cref="NetworkDevice"/> on the
/// graph.
/// </summary>
public class DeviceNode : GraphNode
{
    /// <summary>
    /// The <see cref="NetworkDevice"/> this <see cref="DeviceNode"/> represents
    /// in the graph.
    /// </summary>
    public NetworkDevice NetworkDevice { get; set; }
    public long NetworkDeviceId { get; set; }
    
    /// <summary>
    /// The <see cref="IslandGroup"/> this <see cref="DeviceNode"/> is a member of. If not
    /// set it is not part of a <see cref="IslandGroup"/>.
    /// </summary>
    public IslandGroup? IslandGroup { get; set; }
    public long? IslandGroupId { get; set; }

#pragma warning disable CS8618
    public DeviceNode(long layoutResetId, string displayName, long networkDeviceId, long? islandGroupId) : base(layoutResetId, displayName)
#pragma warning restore CS8618
    {
        NetworkDeviceId = networkDeviceId;
        IslandGroupId = islandGroupId;
    }

    public DeviceNode(LayoutPreset layoutPreset, string displayName, NetworkDevice networkDevice, IslandGroup? islandGroup) : base(layoutPreset, displayName)
    {
        NetworkDevice = networkDevice;
        IslandGroup = islandGroup;
    }

#pragma warning disable CS8618
    public DeviceNode() { }
#pragma warning restore CS8618
}