using Fennec.Parsers;
using Fennec.Processing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fennec.Database.Domain;

/// <summary>
///     Specifies conditions that are used when initially querying the database.
/// </summary>
/// <remarks>
///     These are restrictive as they are supposed to help speed up at the database level.
///     For detailed control use <see cref="ILayer" /> instead.
/// </remarks>
public class QueryConditions
{
    /// <summary>
    ///     If duplicate flagged <see cref="SingleTrace" />s should be included in the result.
    /// </summary>
    [BsonElement("allowDuplicates")]
    public bool? AllowDuplicates { get; set; }

    /// <summary>
    ///     A list of allowed flow protocols that should be included in the result.
    /// </summary>
    /// <remarks>This is intended to help differentiate between SFlow and other flow protocols.</remarks>
    [BsonElement("flowProtocolsWhitelist")]
    public FlowProtocol[]? FlowProtocolsWhitelist { get; set; } 

    /// <summary>
    ///     A list of allowed only those data carrying protocols that should be included in the result. If null this
    ///     condition is ignored.
    /// </summary>
    [BsonElement("dataProtocolsWhitelist")]
    public DataProtocol[]? DataProtocolsWhitelist { get; set; }

    /// <summary>
    ///     If specified a list of allowed source and destination ports that should be included in the result. If either
    ///     source or destination port matches any of the ports in the list the trace is included in the result.
    /// </summary>
    [BsonElement("portsWhitelist")]
    public int[]? PortsWhitelist { get; set; }
}

public record QueryConditionsDto(
    bool? AllowDuplicates, 
    FlowProtocol[]? FlowProtocolsWhitelist, 
    DataProtocol[]? DataProtocolsWhitelist, 
    int[]? PortsWhitelist);

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
    ///    The conditions that should be applied when querying the database.
    /// </summary>
    [BsonElement("queryConditions")] 
    public QueryConditions QueryConditions { get; set; } = new();

    /// <summary>
    ///     The layers applied to the graph before returning to the frontend.
    /// </summary>
    [BsonElement("layers")]
    public IList<ILayer> Layers { get; set; } = new List<ILayer>();
}

/// <summary>
///     Represents a simplified layout with its layers removed.
/// </summary>
/// <param name="Name"></param>
/// <param name="LayerCount"></param>
public record ShortLayoutDto(string Name, int LayerCount);

public record FullLayoutDto(string Name, QueryConditionsDto QueryConditions, IList<ShortLayerDto> Layers);