using Microsoft.EntityFrameworkCore;

namespace Fennec.Database.Domain.Layout;

[Owned]
public record PositionInfo(int X, int Y);

/// <summary>
/// On the frontend this represents a single node to be drawn on the graph. It
/// is part of one <see cref="Layout"/>.
/// </summary>
public class GraphNode
{
#pragma warning disable CS8618
    public GraphNode(long layoutId, string displayName)
#pragma warning restore CS8618
    {
        LayoutId = layoutId;
        DisplayName = displayName;
    }

    public GraphNode(Layout layout, string displayName)
    {
        Layout = layout;
        DisplayName = displayName;
    }

#pragma warning disable CS8618
    public GraphNode()
    {
    }
#pragma warning restore CS8618

    public long Id { get; set; }

    /// <summary>
    /// The <see cref="Layout"/> this <see cref="GraphNode"/> belongs to.
    /// </summary>
    public Layout Layout { get; set; }

    public long LayoutId { get; set; }

    /// <summary>
    /// A name given by the user to this <see cref="GraphNode"/>.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     The position of this <see cref="GraphNode" /> on the graph.
    /// </summary>
    public PositionInfo? Position { get; set; }

    /// <summary>
    ///     Whether this <see cref="GraphNode" /> is hidden on the graph.
    /// </summary>
    public bool IsVisible { get; set; } = true;
}