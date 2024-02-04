using Fennec.Database.Domain.Layers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Database.Domain;

/// <summary>
///     Represents a list of steps that should be taken before sending data to the frontend.
/// </summary>
public class Layout
{
    public Layout(string name)
    {
        Name = name;
        Layers = new List<ILayer>();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected Layout() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    ///     The name of the layout as can be selected by the user.
    /// </summary>
    [BsonElement("name")]
    public string Name { get; set; }

    /// <summary>
    ///     The layers applied to the graph before returning to the frontend.
    /// </summary>
    [BsonElement("layers")]
    public IList<ILayer> Layers { get; set; }
}

/// <summary>
///     Represents a simplified layout with its layers removed.
/// </summary>
/// <param name="Name"></param>
/// <param name="LayerCount"></param>
public record ShortLayoutDto(string Name, int LayerCount);

public record FullLayoutDto(string Name, IList<ShortLayerDto> Layers);