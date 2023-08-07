using System.Net;

namespace Fennec.Database.Domain.Technical;

public enum TraceProtocol
{
    Udp,
    Tcp
}

/// <summary>
/// Processed information received using Netflow representing the communication between two devices.
/// </summary>
public class SingleTrace
{
    public long Id { get; set; }

    /// <summary>
    /// The <see cref="IPAddress"/> of the device which sent this information.
    /// </summary>
    public IPAddress ExporterIp { get; set; }
    
    /// <summary>
    /// Time when this information was received.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// What protocol used during communication.
    /// </summary>
    public TraceProtocol Protocol { get; set; }

    /// <summary>
    /// The source address of the flow set.
    /// </summary>
    public NetworkHost SourceHost { get; set; }
    public long SourceHostId { get; set; }
    public int SourcePort { get; set; }

    /// <summary>
    /// The destination of the flow set.
    /// </summary>
    public NetworkHost DestinationHost { get; set; }
    public long DestinationHostId { get; set; }
    public int DestinationPort { get; set; }

    /// <summary>
    /// How many bytes were sent.
    /// </summary>
    public int ByteCount { get; set; }
    
    /// <summary>
    /// The amount of packets transmitted.
    /// </summary>
    public int PacketCount { get; set; }

    public SingleTrace(IPAddress exporterIp, DateTimeOffset timestamp, TraceProtocol protocol, NetworkHost sourceHost, int sourcePort, NetworkHost destinationHost, int destinationPort, int byteCount, int packetCount)
    {
        ExporterIp = exporterIp;
        Timestamp = timestamp.ToOffset(TimeSpan.Zero);
        Protocol = protocol;
        SourceHost = sourceHost;
        SourcePort = sourcePort;
        DestinationHost = destinationHost;
        DestinationPort = destinationPort;
        ByteCount = byteCount;
        PacketCount = packetCount;
    }

#pragma warning disable CS8618
    public SingleTrace(long id, IPAddress exporterIp, DateTimeOffset timestamp, TraceProtocol protocol, long sourceHostId, int sourcePort, long destinationHostId, int destinationPort, int byteCount, int packetCount)
#pragma warning restore CS8618
    {
        Id = id;
        ExporterIp = exporterIp;
        Timestamp = timestamp;
        Protocol = protocol;
        SourceHostId = sourceHostId;
        SourcePort = sourcePort;
        DestinationHostId = destinationHostId;
        DestinationPort = destinationPort;
        ByteCount = byteCount;
        PacketCount = packetCount;
    }

#pragma warning disable CS8618
    public SingleTrace() { }
#pragma warning restore CS8618
}