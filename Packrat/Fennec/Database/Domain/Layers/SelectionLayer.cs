namespace Fennec.Database.Domain.Layers;

/// <summary>
/// Includes or excludes certain nodes based on their IP address.
/// </summary>
public class SelectionLayer : ILayoutLayer
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