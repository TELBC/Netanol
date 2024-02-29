using Fennec.Database.Domain;

namespace Fennec.Processing.Graph;

public class TraceEdge
{
    public TraceEdge(TraceNodeKey source, TraceNodeKey target, ushort sourcePort, ushort targetPort,
        DataProtocol dataProtocol, ulong packetCount, ulong byteCount)
    {
        Source = source;
        Target = target;
        SourcePort = sourcePort;
        TargetPort = targetPort;
        DataProtocol = dataProtocol;
        PacketCount = packetCount;
        ByteCount = byteCount;
    }

    public TraceNodeKey Source { get; set; }
    public ushort SourcePort { get; set; }

    public TraceNodeKey Target { get; set; }
    public ushort TargetPort { get; set; }

    public DataProtocol DataProtocol { get; set; }
    public ulong PacketCount { get; set; }
    public ulong ByteCount { get; set; }
    
    public float? Width { get; set; }
    public string? HexColor { get; set; }

    /// <summary>
    ///     Creates a new key for this edge.
    /// </summary>
    public TraceEdgeKey Key => new(Source, SourcePort, Target, TargetPort, DataProtocol);
}

public record TraceEdgeDto(
    string Id,
    string Source,
    string Target,
    DataProtocol DataProtocol,
    ulong PacketCount,
    ulong ByteCount,
    float? Width,
    string? HexColor);