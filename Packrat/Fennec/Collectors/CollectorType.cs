namespace Fennec.Collectors;

public enum CollectorType
{
    Netflow9,
    Ipfix
}

public class CollectorSingleTraceMetrics
{
    public ulong PacketCount;
    public ulong ByteCount;
}