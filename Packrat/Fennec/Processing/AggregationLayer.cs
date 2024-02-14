using Fennec.Database;
using Fennec.Processing.Graph;

namespace Fennec.Processing;

/// <summary>
/// Groups nodes by their id.
/// </summary>
public class AggregationLayer : ILayer
{
    public string Type { get; set; } = LayerType.Aggregation;
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    public string Description => "Not implemented";
    
    public void Execute(ITraceGraph graph)
    {
        throw new NotImplementedException();
    }
}