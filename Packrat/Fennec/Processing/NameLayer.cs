using Fennec.Database;
using Fennec.Processing.Graph;

namespace Fennec.Processing;

/// <summary>
/// Stores and sets names for nodes.
/// </summary>
public class NameLayer : ILayer
{
    public string Type { get; set; } = LayerType.Name;
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    public string Description => "Not implemented";
    
    public void Execute(ITraceGraph graph)
    {
        throw new NotImplementedException();
    }
}