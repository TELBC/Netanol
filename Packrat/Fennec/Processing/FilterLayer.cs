using Fennec.Processing.Graph;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Processing;

/// <summary>
/// Includes or excludes certain nodes based on their IP address.
/// </summary>
public class FilterLayer : ILayer
{
    [BsonElement("type")]
    public string Type { get; set; } = LayerType.Filter;
    
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("enabled")]
    public bool Enabled { get; set; }

    [BsonElement("filterList")] 
    public FilterList FilterList { get; set; } 
    
    [BsonIgnore]
    public string Description
    {
        get
        {
            var impl = FilterList.ImplicitInclude ? "Include" : "Exclude";
            var cond = FilterList.Conditions.Count == 1 ? "Condition" : "Conditions";
            return $"{FilterList.Conditions.Count} {cond}, Implicit {impl}";   
        }
    }

    public FilterLayer(string? name, bool enabled, FilterList filterList)
    {
        Name = name;
        Enabled = enabled;
        FilterList = filterList;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected FilterLayer() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public void Execute(ITraceGraph graph, IServiceProvider _)
    {
        FilterList.Filter(graph);
    }
}

public record FilterLayerDto(string Type, string? Name, bool Enabled, FilterListDto FilterList) : ILayerDto;
