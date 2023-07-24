using System.Net;
using Tapas.Models;

namespace Tapas.Database.Dto
{

    public class SingleTraceDto
    {
        public SingleTraceDto(TraceProtocol protocol = default, string sourceIpAddress = null, int sourcePort = default, string destinationIpAddress = null, int destinationPort = default)
        {
            Protocol = protocol;
            SourceIpAddress = sourceIpAddress;
            SourcePort = sourcePort;
            DestinationIpAddress = destinationIpAddress;
            DestinationPort = destinationPort;
        }
        
        public TraceProtocol Protocol { get; set; }
        public string SourceIpAddress { get; set; } 
        public int SourcePort { get; set; }
        public string DestinationIpAddress { get; set; } 
        public int DestinationPort { get; set; }
    }
}