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
    
    public IPAddress ExporterIp { get; }
    
    public TraceProtocol Protocol { get; }
    
    public IPAddress SourceIpAddress { get; }
    public int SourcePort { get; }
    
    public IPAddress DestinationIpAddress { get; }
    public int DestinationPort { get; }

    public SingleTrace(TraceProtocol protocol, IPAddress exporterIp, IPAddress sourceIpAddress, int sourcePort, IPAddress destinationIpAddress, int destinationPort)
    {
        Protocol = protocol;
        ExporterIp = exporterIp;
        SourceIpAddress = sourceIpAddress;
        SourcePort = sourcePort;
        DestinationIpAddress = destinationIpAddress;
        DestinationPort = destinationPort;
    }

#pragma warning disable CS8618
    public SingleTrace() { }
#pragma warning restore CS8618
}