using Fennec.Database.Domain.Layers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Database.Domain;

/// <summary>
/// Represents a list of steps that should be taken before sending data to the frontend.
/// </summary>
public class Layout
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    /// <summary>
    /// The name of the layout as can be selected by the user.
    /// </summary>
    [BsonElement("name")]
    public string Name { get; set; }
    
    /// <summary>
    /// The layers 
    /// </summary>
    [BsonElement("layers")]
    public ILayoutLayer[] Layers { get; set; }
}