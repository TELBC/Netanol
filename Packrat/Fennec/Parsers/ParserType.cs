namespace Fennec.Parsers;

public enum ParserType
{
    Netflow9,
    Ipfix,
    All
}

public class CollectorSingleTraceMetrics
{
    public ulong PacketCount;
    public ulong ByteCount;
}