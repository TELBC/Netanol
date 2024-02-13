using System.Net.Sockets;
using DotNetFlow.Sflow;
using Fennec.Database;

namespace Fennec.Parsers;

public class SflowParser : IParser
{
    public IEnumerable<TraceImportInfo> Parse(UdpReceiveResult result)
    {
        using var stream = new MemoryStream(result.Buffer);
        var reader = new SflowReader();
        
        var header = reader.ReadHeader(stream);
        var samples = reader.ReadSamples(stream, header.NumSamples);

        Console.WriteLine();


        return new List<TraceImportInfo>();
    }
}