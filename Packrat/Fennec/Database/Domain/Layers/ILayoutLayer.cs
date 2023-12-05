using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Fennec.Database.Domain.Layers;

public enum LayerType
{
    Filter,
    Aggregation,
    Naming,
    Positioning
}

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
    /// The type of the layer.
    /// </summary>
    public LayerType Type { get; set; }
    
    /// <summary>
    /// Sets whether this layer should be executed or not.
    /// </summary>
    public bool Enabled { get; set; }
    
    public void ExecuteLayer();

    public object GetPreview();

    public object GetFullView();
}

public class LayoutLayerSerializer : SerializerBase<ILayoutLayer>
{
    public override ILayoutLayer Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var document = BsonDocumentSerializer.Instance.Deserialize(context);
        var type = document["Type"].AsInt32;

        return (LayerType)type switch
        {
            LayerType.Filter => BsonSerializer.Deserialize<FilterLayer>(document),
            LayerType.Aggregation => BsonSerializer.Deserialize<AggregationLayer>(document),
            LayerType.Naming => BsonSerializer.Deserialize<NamingLayer>(document),
            LayerType.Positioning => BsonSerializer.Deserialize<PositioningLayer>(document),
            _ => throw new NotSupportedException($"Unknown layer type: {type}")
        };
    }
}