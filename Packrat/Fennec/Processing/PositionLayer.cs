using Fennec.Database;
using Fennec.Processing.Graph;

namespace Fennec.Processing;

/// <summary>
/// Sets and stores the coordinates of nodes to remove the need for rerunning simulations on the frontend.
/// </summary>
public class PositionLayer : ILayer
{
    public string Type { get; set; } = LayerType.Position;
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    public string Description => "Not implemented";
    
    public void Execute(ITraceGraph graph)
    {
        throw new NotImplementedException();
    }
}