namespace Fennec.Database.Domain.Layout;

/// <summary>
/// On the frontend this represents a single node to be drawn on the graph. It
/// is part of one <see cref="Layout"/>.
/// </summary>
public class GraphNode
{
#pragma warning disable CS8618
    public GraphNode(long layoutResetId, string displayName)
#pragma warning restore CS8618
    {
        LayoutResetId = layoutResetId;
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

    public long LayoutResetId { get; set; }

    /// <summary>
    /// A name given by the user to this <see cref="GraphNode"/>.
    /// </summary>
    public string DisplayName { get; set; }
}