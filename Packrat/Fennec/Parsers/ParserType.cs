namespace Fennec.Parsers;

public enum ParserType
{
    Netflow9,
    Ipfix,
    Netflow5
}

public class CollectorSingleTraceMetrics
{
    public ulong PacketCount;
    public ulong ByteCount;
}