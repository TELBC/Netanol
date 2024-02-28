using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Processing.Graph;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Processing;

/// <summary>
///     A list of conditions with either an implicit include or exclude at the end.
/// </summary>
public class FilterList
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FilterList() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public FilterList(bool implicitInclude, List<FilterCondition> conditions)
    {
        ImplicitInclude = implicitInclude;
        Conditions = conditions;
    }

    [BsonElement("implicitInclude")] 
    public bool ImplicitInclude { get; set; }

    [BsonElement("conditions")] 
    public List<FilterCondition> Conditions { get; set; }

    public void Filter(ITraceGraph graph)
    {
        graph.FilterEdges(edge =>
        {
            var condition = Conditions.FirstOrDefault(condition => condition.MatchesTraceEdge(edge));
            return condition?.Include ?? ImplicitInclude;
        });
    }
}

public record FilterListDto(List<FilterConditionDto> Conditions, bool ImplicitInclude);

/// <summary>
///     Matches a single <see cref="AggregateTrace" /> and includes information whether to include or exclude it.
/// </summary>
/// <remarks>
///     See the wiki for a detailed breakdown of its inner workings.
/// </remarks>
public class FilterCondition
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FilterCondition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public FilterCondition(byte[] sourceAddress, byte[] sourceAddressMask, ushort? sourcePort, byte[] destinationAddress, byte[] destinationAddressMask, ushort? destinationPort, DataProtocol? protocol, bool include)
    {
        SourceAddress = sourceAddress;
        SourceAddressMask = sourceAddressMask;
        SourcePort = sourcePort;
        DestinationAddress = destinationAddress;
        DestinationAddressMask = destinationAddressMask;
        DestinationPort = destinationPort;
        Protocol = protocol;
        Include = include;
    }

    // TODO: should we make the source and destination matching optional with a null?
    // TODO: just noticed big risk! the source address is never masked
    [BsonElement("sourceAddress")] 
    public byte[] SourceAddress { get; set; }

    [BsonElement("sourceAddressMask")] 
    public byte[] SourceAddressMask { get; set; }

    [BsonElement("sourcePort")] 
    public ushort? SourcePort { get; set; }

    [BsonElement("destinationAddress")] 
    public byte[] DestinationAddress { get; set; }

    [BsonElement("destinationAddressMask")]
    public byte[] DestinationAddressMask { get; set; }

    [BsonElement("destinationPort")] 
    public ushort? DestinationPort { get; set; }

    [BsonElement("protocol")] 
    public DataProtocol? Protocol { get; set; }

    [BsonElement("include")] 
    public bool Include { get; set; }

    private static IEnumerable<byte> Combine(IReadOnlyList<byte> address, IReadOnlyList<byte> mask)
    {
        var res = new byte[address.Count];
        for (var i = 0; i < address.Count; i++)
            res[i] = (byte)(address[i] & mask[i]);
        return res;
    }

    public bool MatchesTraceEdge(TraceEdge edge)
    {
        // All statements need to match so we can not return true until the end
        
        // Does the source address match?
        var maskedSource = Combine(edge.Source.Address.GetAddressBytes(), SourceAddressMask);
        if (!maskedSource.SequenceEqual(SourceAddress))
            return false;

        // Does the source port match?
        if (SourcePort.HasValue && SourcePort != edge.SourcePort)
            return false;

        // Does the destination address match?
        var maskedDestination = Combine(edge.Target.Address.GetAddressBytes(), DestinationAddressMask);
        if (!maskedDestination.SequenceEqual(DestinationAddress))
            return false;

        // Does the destination port match?
        if (DestinationPort.HasValue && DestinationPort != edge.TargetPort)
            return false;
        
        // Does the protocol match?
        if (Protocol.HasValue && Protocol != edge.DataProtocol)
            return false;

        return true;
    }
}

public record FilterConditionDto(
    string SourceAddress,
    string SourceAddressMask,
    string? SourcePort,
    string DestinationAddress,
    string DestinationAddressMask,
    string? DestinationPort,
    string? Protocol,
    bool Include);