namespace Fennec.Database.Domain.Layout;

/// <summary>
/// On the frontend this represents a single node to be drawn on the graph. It
/// is part of one <see cref="LayoutPreset"/>.
/// </summary>
public class GraphNode
{
    public long Id { get; set; }

    /// <summary>
    /// The <see cref="LayoutPreset"/> this <see cref="GraphNode"/> belongs to.
    /// </summary>
    public LayoutPreset LayoutPreset { get; set; }
    public long LayoutResetId { get; set; }
    
    /// <summary>
    /// A name given by the user to this <see cref="GraphNode"/>.
    /// </summary>
    public string DisplayName { get; set; }

#pragma warning disable CS8618
    public GraphNode(long layoutResetId, string displayName)
#pragma warning restore CS8618
    {
        LayoutResetId = layoutResetId;
        DisplayName = displayName;
    }

    public GraphNode(LayoutPreset layoutPreset, string displayName)
    {
        LayoutPreset = layoutPreset;
        DisplayName = displayName;
    }

#pragma warning disable CS8618
    public GraphNode() { }
#pragma warning restore CS8618
}