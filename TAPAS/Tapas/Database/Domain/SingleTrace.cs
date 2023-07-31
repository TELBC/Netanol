using System.Net;

namespace Tapas.Models;

public enum TraceProtocol
{
    Udp,
    Tcp
}

public class SingleTrace
{
    public long Id { get; set; }

    public IPAddress ExporterIp { get; set; }

    public TraceProtocol Protocol { get; set; }

    public IPAddress SourceIpv4Address { get; set; }

    public IPAddress SourceIpv6Address { get; set; }
    public int SourcePort { get; set; }

    public IPAddress DestinationIpv4Address { get; set; }
    public IPAddress DestinationIpv6Address { get; set; }
    public int DestinationPort { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public SingleTrace(TraceProtocol protocol, IPAddress exporterIp, IPAddress sourceIpv4Address,
        IPAddress sourceIpv6Address, int sourcePort, IPAddress destinationIpv4Address, IPAddress destinationIpv6Address,
        int destinationPort, DateTimeOffset timestamp)
    {
        Protocol = protocol;
        ExporterIp = exporterIp;
        SourceIpv4Address = sourceIpv4Address;
        SourceIpv6Address = sourceIpv6Address;
        SourcePort = sourcePort;
        DestinationIpv4Address = destinationIpv4Address;
        DestinationIpv6Address = destinationIpv6Address;
        DestinationPort = destinationPort;
        Timestamp = timestamp.ToOffset(TimeSpan.Zero);
    }

#pragma warning disable CS8618
    public SingleTrace()
    {
    }

#pragma warning restore CS8618
}