namespace Tapas.Models;

/// <summary>
/// A grouping of <see cref="DeviceNode"/>s which are meant to be draw closely together
/// on the graph.
/// </summary>
public class IslandGroup
{
    public long Id { get; set; }

    /// <summary>
    /// All <see cref="DeviceNode"/>s that are part of this <see cref="IslandGroup"/>.
    /// </summary>
    public ICollection<DeviceNode> Members { get; set; } = default!;

    public IslandGroup() { }
}