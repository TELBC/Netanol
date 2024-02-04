namespace Fennec.Database.Graph;

public class Edge
{
    public string Source { get; set; }
    public string Target { get; set; }
    public ulong PacketCount { get; set; }
    public ulong ByteCount { get; set; }

    public Edge(string source, string target, ulong packetCount, ulong byteCount)
    {
        Source = source;
        Target = target;
        PacketCount = packetCount;
        ByteCount = byteCount;
    }
}