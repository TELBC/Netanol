using System.Net.Sockets;
using DotNetFlow.Sflow;
using Fennec.Database;

namespace Fennec.Parsers;

public class SflowParser : IParser
{
    public IEnumerable<TraceImportInfo> Parse(UdpReceiveResult result)
    {
        var datagram = result.Buffer;
        var reader = new SflowReader();
        var header = reader.ReadHeader(datagram);
        var sample = reader.ReadSample(datagram);
        
        
        return new List<TraceImportInfo>();
    }
}