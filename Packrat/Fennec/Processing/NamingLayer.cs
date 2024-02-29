using System.Net;
using Fennec.Processing.Graph;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Processing;

public class NamingAssigner
{
    public NamingAssigner(IpAddressMatcher matcher, string? name)
    {
        Matcher = matcher;
        Name = name;
    }

    public NamingAssigner()
    {
    }

    [BsonElement("matcher")]
    public IpAddressMatcher Matcher { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }
}

public record NamingAssignerDto(IpAddressMatcherDto Matcher, string? Name);

/*
 * Possible names are:
 *  - The IP address
 *  - The DNS name
 *  - A user defined name
 */

/// <summary>
///     Store and set names of <see cref="TraceNode" />.
/// </summary>
public class NamingLayer : ILayer
{
    [BsonElement("overwriteUsingDns")]
    public bool OverwriteWithDns { get; set; }

    [BsonElement("matchers")]
    public List<NamingAssigner> Matchers { get; set; } = new();

    [BsonElement("type")]
    public string Type { get; set; } = LayerType.Naming;

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("enabled")]
    public bool Enabled { get; set; }

    public string Description
    {
        get
        {
            var s = Matchers.Count == 1 ? "" : "s";
            return $"{Matchers.Count} Matcher{s}";
        }
    }

    /// <summary>
    ///     Naming logic for the nodes in the graph.
    ///     1. Get the assigner for the address <br />
    ///     2. If the assigner is defined and <see cref="NamingAssigner.Include" /> is false, skip the node <br />
    ///     3. If the node has a DNS name and <see cref="OverwriteWithDns" /> is true, overwrite the name with the DNS name
    ///     <br />
    ///     4. If the assigner is undefined or <see cref="NamingAssigner.Name" /> is null, skip the node <br />
    ///     5. Assign <see cref="NamingAssigner.Name" /> to the node
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="_"></param>
    public void Execute(ITraceGraph graph, IServiceProvider _)
    {
        foreach (var node in graph.Nodes.Select(graphNode => graphNode.Value))
        {
            // Yuk!
            var assigner = GetAssigner(node.Address);
            if (assigner is null)
            {
                if (node.DnsName is not null && OverwriteWithDns)
                    node.Name = node.DnsName;
                continue;
            }

            if (assigner.Matcher is { Include: false })
                continue;

            if (node.DnsName is not null && OverwriteWithDns)
                node.Name = node.DnsName;

            if (assigner is null or { Name: null })
                continue;

            node.Name = assigner.Name;
        }
    }

    private NamingAssigner? GetAssigner(IPAddress address)
    {
        return Matchers.FirstOrDefault(matcher => matcher.Matcher.Match(address.GetAddressBytes()));
    }
}

public record NamingLayerDto(
    string Type,
    bool Enabled,
    string? Name,
    bool OverwriteWithDns,
    List<NamingAssignerDto> Matchers) : ILayerDto;