using Microsoft.EntityFrameworkCore;

namespace Fennec.Database.Domain.Layout;

/// <summary>
/// A layout defined by the user to be able to switch between when displaying
/// the graph.
/// </summary>
[Index(nameof(Name), IsUnique = true)]
public class Layout
{
    public Layout(string name)
    {
        Name = name;
    }

#pragma warning disable CS8618
    public Layout()
    {
    }
#pragma warning restore CS8618
    public long Id { get; set; }

    /// <summary>
    /// The name of the <see cref="Layout"/> as defined by the user.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// All <see cref="GraphNodes"/> that are part of this layout.
    /// </summary>
    public ICollection<GraphNode> GraphNodes { get; set; } = default!;
}