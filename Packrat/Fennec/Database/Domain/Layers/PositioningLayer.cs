namespace Fennec.Database.Domain.Layers;

/// <summary>
/// Sets and stores the coordinates of nodes to remove the need for rerunning simulations on the frontend.
/// </summary>
public class PositioningLayer : ILayoutLayer
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