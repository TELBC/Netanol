namespace Fennec.Parsers;

public enum ParserType
{
    Netflow9,
    Ipfix
}

public class CollectorSingleTraceMetrics
{
    public ulong PacketCount;
    public ulong ByteCount;
}