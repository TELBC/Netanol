namespace Fennec.Database.Domain.Layers;

/// <summary>
/// Stores and sets names for nodes.
/// </summary>
public class NameLayer : ILayer
{
    public string Type { get; set; } = LayerType.Name;
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    public string Description => "Not implemented";
    
    public void Execute(ref List<AggregateTrace> aggregateTraces)
    {
        throw new NotImplementedException();
    }
}