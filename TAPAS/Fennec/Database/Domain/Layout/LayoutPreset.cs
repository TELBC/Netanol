﻿namespace Fennec.Database.Domain.Layout;

/// <summary>
/// A layout defined by the user to be able to switch between when displaying
/// the graph.
/// </summary>
public class LayoutPreset
{
    public long Id { get; set; }
    
    /// <summary>
    /// The name of the <see cref="LayoutPreset"/> as defined by the user.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// All <see cref="GraphNodes"/> that are part of this layout.
    /// </summary>
    public ICollection<GraphNode> GraphNodes { get; set; } = default!;

    public LayoutPreset(string name)
    {
        Name = name;
    }

#pragma warning disable CS8618
    public LayoutPreset() { }
#pragma warning restore CS8618
}