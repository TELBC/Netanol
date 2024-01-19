using System.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Database.Domain;

public enum TraceProtocol
{
    Unknown,
    Udp,
    Tcp
}

/// <summary>
/// Details about one host of a two sided communication represented by a <see cref="SingleTrace"/>.
/// </summary>
public class SingleTraceEndpoint
{
    /// <summary>
    /// The <see cref="IpAddress"/> of the host represented by in a byte array.
    /// </summary>
    [BsonElement("ipBytes")]
    public byte[] IpBytes { get; set; }

    /// <summary>
    /// The port used by the host.
    /// </summary>
    [BsonElement("port")]
    public ushort Port { get; set; }

    /// <summary>
    /// The DNS name of the host at the time of the trace.
    /// </summary>
    [BsonElement("dnsName")]
    public string? DnsName { get; set; }

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

    [BsonIgnore] public IPAddress IpAddress => new(IpBytes);
}

/// <summary>
/// Processed information received using Netflow representing the communication between two devices.
/// </summary>
public class SingleTrace
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Time when this information was received.
    /// </summary>
    [BsonElement("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// What protocol was used during communication.
    /// </summary>
    [BsonElement("protocol")]
    public TraceProtocol Protocol { get; set; }

    /// <summary>
    /// Information about the source of the communication between two devices.
    /// </summary>
    [BsonElement("source")]
    public SingleTraceEndpoint Source { get; set; }

    /// <summary>
    /// Information about the destination of the communication between two devices.
    /// </summary>
    [BsonElement("destination")]
    public SingleTraceEndpoint Destination { get; set; }

    /// <summary>
    /// The amount of bytes transmitted.
    /// </summary>
    [BsonElement("byteCount")]
    public ulong ByteCount { get; set; }

    /// <summary>
    /// The amount of packets transmitted.
    /// </summary>
    [BsonElement("packetCount")]
    public ulong PacketCount { get; set; }

    public SingleTrace(DateTimeOffset timestamp, TraceProtocol protocol, SingleTraceEndpoint source,
        SingleTraceEndpoint destination, ulong byteCount, ulong packetCount)
    {
        Timestamp = timestamp;
        Protocol = protocol;
        Source = source;
        Destination = destination;
        ByteCount = byteCount;
        PacketCount = packetCount;
    }

#pragma warning disable CS8618
    public SingleTrace() { }
#pragma warning restore CS8618
}