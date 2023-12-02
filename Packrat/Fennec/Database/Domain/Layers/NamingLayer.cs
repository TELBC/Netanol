namespace Fennec.Database.Domain.Layers;

/// <summary>
/// Stores and sets names for nodes.
/// </summary>
public class NamingLayer : ILayoutLayer
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