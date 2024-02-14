using System.Net;
using Fennec.Database.Domain;

namespace Fennec.Processing.Graph;

public class TraceEdge
{
    public IPAddress Source { get; set; }
    public ushort SourcePort { get; set; }
    
    public IPAddress Target { get; set; }
    public ushort TargetPort { get; set; }
    
    public DataProtocol DataProtocol { get; set; }
    public ulong PacketCount { get; set; }
    public ulong ByteCount { get; set; }

    public TraceEdge(IPAddress source, IPAddress target, ushort sourcePort, ushort targetPort, DataProtocol dataProtocol, ulong packetCount, ulong byteCount)
    {
        Source = source;
        Target = target;
        SourcePort = sourcePort;
        TargetPort = targetPort;
        DataProtocol = dataProtocol;
        PacketCount = packetCount;
        ByteCount = byteCount;
    }
}

public record TraceEdgeDto(
    string Source,
    string Target,
    DataProtocol DataProtocol,
    ulong PacketCount,
    ulong ByteCount);
