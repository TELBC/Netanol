namespace Fennec.Database.Domain.Layers;

/// <summary>
/// Groups nodes by their id.
/// </summary>
public class AggregationLayer : ILayoutLayer
{
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    
    public void ExecuteLayer()
    {
        throw new NotImplementedException();
    }

    public object GetPreview()
    {
        throw new NotImplementedException();
    }

    public object GetFullView()
    {
        throw new NotImplementedException();
    }
}