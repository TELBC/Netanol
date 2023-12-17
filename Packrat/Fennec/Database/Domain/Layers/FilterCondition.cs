using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Database.Domain.Layers;

/// <summary>
/// A list of conditions with either an implicit include or exclude at the end. 
/// </summary>
public class FilterList
{
    [BsonElement("implicitInclude")]
    public bool ImplicitInclude { get; set; }
    
    [BsonElement("conditions")]
    public List<FilterCondition> Conditions { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FilterList() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public FilterList(bool implicitInclude, List<FilterCondition> conditions)
    {
        ImplicitInclude = implicitInclude;
        Conditions = conditions;
    }

    public void Filter(ref List<AggregateTrace> aggregateTraces)
    {
        for (var i = 0; i < aggregateTraces.Count; i++)
        {
            var aggregateTrace = aggregateTraces[i];
            foreach (var condition in Conditions)
            {
                if (!condition.Match(aggregateTrace))
                    continue;

                if (!condition.Include)
                {
                    aggregateTraces.RemoveAt(i);
                    i--;
                }
                    
                goto NextTrace;
            }
            
            if (ImplicitInclude)
                continue;
            aggregateTraces.RemoveAt(i);
            i--;
            
            NextTrace: ;
        }
    }
}

public record FilterListDto(List<FilterConditionDto> Conditions, bool ImplicitInclude);


/// <summary>
///     Matches a single <see cref="AggregateTrace" /> and includes information whether to include or exclude it.
/// </summary>
/// <remarks>
///     <para>
///         For example, if we want to check whether a <see cref="AggregateTrace" /> has
///         a source address in the subnet of `192.168.100.0/24` we would set the source address
///         to `192.168.100.0` and the mask to `255.255.255.0`. As we care only about the source we can
///         match every address on the destination by setting them as `0.0.0.0` and `0.0.0.0`.
///     </para>
///     <para>
///         Assume that we now receive the following incoming trace: `192.168.100.30 -> 10.20.30.40`.
///         The algorithm would first check the source address by masking the trace source address
///         with the mask like so:
///     </para>
///     <br />
///     <see cref="AggregateTrace.SourceIpBytes" />:       192.168.100.30 <br />
///     <see cref="SourceAddressMask" />:      255.255.255.0 <br />
///     Intermediate Result:        192.168.100.0 <br />
///     <para>
///         This is done using an AND operator on each byte in the address. Next this result is
///         compared to the source address of the condition. If they are equal the algorithm
///         considers this a match.
///     </para>
///     <br />
///     Intermediate Result:            192.168.100.0
///     <see cref="SourceAddress" />:    192.168.100.0
///     This would be considered a match and the algorithm would continue to check the destination. The process
///     stays the same for the destination address.
/// </remarks>
public class FilterCondition
{
    [BsonElement("sourceAddress")] 
    public byte[] SourceAddress { get; set; }

    [BsonElement("sourceAddressMask")] 
    public byte[] SourceAddressMask { get; set; }

    [BsonElement("destinationAddress")] 
    public byte[] DestinationAddress { get; set; }

    [BsonElement("destinationAddressMask")]
    public byte[] DestinationAddressMask { get; set; }

    [BsonElement("include")] 
    public bool Include { get; set; }

    // public long MinPacketCount { get; set; }
    // public long MaxPacketCount { get; set; }
    //
    // public long MinByteCount { get; set; }
    // public long MaxByteCount { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FilterCondition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public FilterCondition(byte[] sourceAddress, byte[] sourceAddressMask, byte[] destinationAddress, byte[] destinationAddressMask, bool include)
    {
        SourceAddress = sourceAddress;
        SourceAddressMask = sourceAddressMask;
        DestinationAddress = destinationAddress;
        DestinationAddressMask = destinationAddressMask;
        Include = include;
    }

    private static IEnumerable<byte> Combine(IReadOnlyList<byte> address, IReadOnlyList<byte> mask)
    {
        var res = new byte[address.Count];
        for (var i = 0; i < address.Count; i++)
            res[i] = (byte)(address[i] & mask[i]);
        return res;
    }

    public bool Match(AggregateTrace trace)
    {
        var maskedSource = Combine(trace.SourceIpBytes, SourceAddressMask);
        if (!maskedSource.SequenceEqual(SourceAddress))
            return false;

        var maskedDestination = Combine(trace.DestinationIpBytes, DestinationAddressMask);
        if (!maskedDestination.SequenceEqual(DestinationAddress))
            return false;

        return true;
    }
}

public record FilterConditionDto(
    string SourceAddress, string SourceAddressMask, 
    string DestinationAddress, string DestinationAddressMask, 
    bool Include);