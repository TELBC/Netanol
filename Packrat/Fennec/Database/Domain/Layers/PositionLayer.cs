namespace Fennec.Database.Domain.Layers;

/// <summary>
/// Sets and stores the coordinates of nodes to remove the need for rerunning simulations on the frontend.
/// </summary>
public class PositionLayer : ILayer
{
    public string Type { get; set; } = LayerType.Position;
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    public string Description => "Not implemented";
    
    public void Execute(ref List<AggregateTrace> aggregateTraces)
    {
        throw new NotImplementedException();
    }
}