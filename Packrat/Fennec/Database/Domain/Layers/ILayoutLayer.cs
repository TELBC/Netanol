namespace Fennec.Database.Domain.Layers;

/// <summary>
/// 
/// </summary>
public interface ILayoutLayer
{
    /// <summary>
    /// An optional name that can be set by the user for easier identification.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Sets whether this layer should be executed or not.
    /// </summary>
    public bool Enabled { get; set; }
    
    public void ExecuteLayer();

    public object GetPreview();

    public object GetFullView();
}