using Tapas.Models;

namespace Tapas
{

    public class SingleTraceDto
    {
        public TraceProtocol Protocol { get; set; }
        
        public string SourceIpAddress { get; set; }
        public int SourcePort { get; set; }
        
        public string DestinationIpAddress { get; set; }
        public int DestinationPort { get; set; }
    }
}