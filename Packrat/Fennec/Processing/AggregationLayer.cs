using System.Net;
using Fennec.Database;
using Fennec.Processing.Graph;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Processing;

/// <summary>
/// Groups nodes by their id.
/// </summary>
public class AggregationLayer : ILayer
{
    [BsonElement("type")]
    public string Type { get; set; } = LayerType.Aggregation;
    
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("enabled")]
    public bool Enabled { get; set; }
    
    [BsonElement("matchers")]
    public List<IpAddressMatcher> Matchers { get; set; }
    
    public string Description => "Not implemented";
    
    public AggregationLayer(string name, bool enabled, List<IpAddressMatcher> matchers)
    {
        Name = name;
        Enabled = enabled;
        Matchers = matchers;
    }
    
#pragma warning disable CS8618 
    public AggregationLayer() { }
#pragma warning restore CS8618
    
    public void Execute(ITraceGraph graph)
    {
        // var nodesToGroup = graph.Nodes.Where((key, value) => true);
        graph.GroupNodes((key, _) =>
        {
            var matcher = GetMatcherForAddress(key);
            if (matcher is not { Include: true })
                return null;
         
            return new IPAddress(matcher.MaskedAddress);
        }, (b, _) => new TraceNode(b, b.ToString()));
    }
    
    private IpAddressMatcher? GetMatcherForAddress(IPAddress address) => 
        Matchers.FirstOrDefault(matcher => matcher.Match(address.GetAddressBytes()));
}

public record AggregationLayerDto(string Type, string? Name, bool Enabled, List<IpAddressMatcherDto> Matchers) : ILayerDto;
