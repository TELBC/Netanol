namespace Fennec.Database.Domain.Layout;

/// <summary>
/// A grouping of <see cref="HostNode"/>s which are meant to be draw closely together
/// on the graph.
/// </summary>
public class IslandGroup
{
    public IslandGroup()
    {
    }

    public long Id { get; set; }

    /// <summary>
    /// All <see cref="HostNode"/>s that are part of this <see cref="IslandGroup"/>.
    /// </summary>
    public ICollection<HostNode> Members { get; set; } = default!;
}