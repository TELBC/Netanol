using Tapas.Models;

namespace Tapas.Database.Dto;
    
public class SingleTraceDto
{
    public string Protocol { get; set; }
    public string SourceIpAddress { get; set; }
    public int SourcePort { get; set; }
    public string DestinationIpAddress { get; set; }
    public int DestinationPort { get; set; }
    
    public int Count { get; set; }
    public SingleTraceDto(string protocol, string sourceIpAddress, int sourcePort, string destinationIpAddress,
        int destinationPort, int count)
    {
        Protocol = protocol;
        SourceIpAddress = sourceIpAddress;
        SourcePort = sourcePort;
        DestinationIpAddress = destinationIpAddress;
        DestinationPort = destinationPort;
        Count = count;

    }
    
}