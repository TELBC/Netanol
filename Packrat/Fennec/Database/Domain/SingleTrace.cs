using System.Net;
using Fennec.Parsers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Database.Domain;

public enum DataProtocol
{
    Unknown = 253, // "Used for experimentation and testing"
    Udp = 17,
    Tcp = 6,
    Icmp = 1,
}

/// <summary>
///     Details about one host of a two sided communication represented by a <see cref="SingleTrace" />.
/// </summary>
public class SingleTraceEndpoint
{
    public SingleTraceEndpoint(IPAddress ipAddress, ushort port, string? dnsName = null)
    {
        IpBytes = ipAddress.GetAddressBytes();
        Port = port;
        DnsName = dnsName;
    }

#pragma warning disable CS8618
    private SingleTraceEndpoint()
    {
    }
#pragma warning restore CS8618
    /// <summary>
    ///     The <see cref="IpAddress" /> of the host represented by in a byte array.
    /// </summary>
    [BsonElement("ipBytes")]
    public byte[] IpBytes { get; set; }

    /// <summary>
    ///     The port used by the host.
    /// </summary>
    [BsonElement("port")]
    public ushort Port { get; set; }

    /// <summary>
    ///     The DNS name of the host at the time of the trace.
    /// </summary>
    [BsonElement("dnsName")]
    public string? DnsName { get; set; }
}

/// <summary>
///     Processed information received using Netflow representing the communication between two devices.
/// </summary>
public class SingleTrace
{
    public SingleTrace(DateTimeOffset timestamp, DataProtocol dataProtocol, FlowProtocol flowProtocol, bool duplicate,
        SingleTraceEndpoint source,
        SingleTraceEndpoint destination, ulong byteCount, ulong packetCount)
    {
        Timestamp = timestamp;
        DataProtocol = dataProtocol;
        FlowProtocol = flowProtocol;
        Duplicate = duplicate;
        Source = source;
        Destination = destination;
        ByteCount = byteCount;
        PacketCount = packetCount;
    }

#pragma warning disable CS8618
    public SingleTrace()
    {
    }
#pragma warning restore CS8618
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    ///     Time when this information was received.
    /// </summary>
    [BsonElement("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    ///     Information about the source of the communication between two devices.
    /// </summary>
    [BsonElement("source")]
    public SingleTraceEndpoint Source { get; set; }

    /// <summary>
    ///     Information about the destination of the communication between two devices.
    /// </summary>
    [BsonElement("destination")]
    public SingleTraceEndpoint Destination { get; set; }

    /// <summary>
    ///     What protocol was used during communication.
    /// </summary>
    [BsonElement("dataProtocol")]
    public DataProtocol DataProtocol { get; set; }

    /// <summary>
    ///     The protocol used to transmit the flow.
    /// </summary>
    [BsonElement("flowProtocol")]
    public FlowProtocol FlowProtocol { get; set; }

    /// <summary>
    ///     Whether when this trace was received it was a duplicate.
    /// </summary>
    [BsonElement("duplicate")]
    public bool Duplicate { get; set; }

    /// <summary>
    ///     The amount of bytes transmitted.
    /// </summary>
    [BsonElement("byteCount")]
    public ulong ByteCount { get; set; }

    /// <summary>
    ///     The amount of packets transmitted.
    /// </summary>
    [BsonElement("packetCount")]
    public ulong PacketCount { get; set; }
}