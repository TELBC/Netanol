using Fennec.Controllers;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Database.Domain.Layers;

/// <summary>
/// Includes or excludes certain nodes based on their IP address.
/// </summary>
public class FilterLayer : ILayer
{
    [BsonElement("type")]
    public string Type { get; set; } = LayerType.Filter;
    
    [BsonElement("name")]
    public string Name { get; set; }
    
    [BsonElement("enabled")]
    public bool Enabled { get; set; }
    
    public void ExecuteLayer()
    {
        throw new NotImplementedException();
    }

    public FilterLayer(string name, bool enabled, FilterList filterList)
    {
        Name = name;
        Enabled = enabled;
        FilterList = filterList;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected FilterLayer() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public void Execute(ref List<AggregateTrace> aggregateTraces)
    {
        throw new NotImplementedException();
    }
}

public record FilterLayerDto(string Type, string Name, bool Enabled, FilterListDto FilterList) : ILayerDto;
