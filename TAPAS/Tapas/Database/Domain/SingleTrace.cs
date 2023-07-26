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
    
    public IPAddress SourceIpAddress { get; set; }
    public int SourcePort { get; set; }
    
    public IPAddress DestinationIpAddress { get; set; }
    public int DestinationPort { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }

    public SingleTrace(TraceProtocol protocol, IPAddress exporterIp, IPAddress sourceIpAddress, int sourcePort, IPAddress destinationIpAddress, int destinationPort, DateTimeOffset timestamp)
    {
        Protocol = protocol;
        ExporterIp = exporterIp;
        SourceIpAddress = sourceIpAddress;
        SourcePort = sourcePort;
        DestinationIpAddress = destinationIpAddress;
        DestinationPort = destinationPort;
        Timestamp = timestamp.ToOffset(TimeSpan.Zero);
    }

#pragma warning disable CS8618
    public SingleTrace() { }
 
#pragma warning restore CS8618
}