using Tapas.Models;

namespace Tapas.Database.Dto;
    
public class SingleTraceDto
{
    public string Protocol { get; set; }
    public string? SourceIpv4Address { get; set; }
    public string? SourceIpv6Address { get; set; }
    public int SourcePort { get; set; }
    public string? DestinationIpv4Address { get; set; }
    public string? DestinationIpv6Address { get; set; }
    public int DestinationPort { get; set; }

    public SingleTraceDto(TraceProtocol protocol = default, string? sourceIpv4Address = null,
        string? sourceIpv6Address = null, int sourcePort = default, string? destinationIpv4Address = null,
        string? destinationIpv6Address = null, int destinationPort = default)
    {
        Protocol = protocol.ToString();
        SourceIpv4Address = sourceIpv4Address;
        SourceIpv6Address = sourceIpv6Address;
        SourcePort = sourcePort;
        DestinationIpv4Address = destinationIpv4Address;
        DestinationIpv6Address = destinationIpv6Address;
        DestinationPort = destinationPort;
    }
}