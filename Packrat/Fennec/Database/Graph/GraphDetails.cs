namespace Fennec.Database.Graph;

public class GraphDetails
{
    public long TotalHostCount { get; set; }
    public long TotalByteCount { get; set; }
    public long TotalPacketCount { get; set; }
    public long TotalTraceCount { get; set; }

    public Dictionary<string, Node> Nodes { get; set; } = new();
    public Dictionary<string, Edge> Edges { get; set; } = new();
}
