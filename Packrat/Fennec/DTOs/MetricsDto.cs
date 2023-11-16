namespace Fennec.DTOs;

public class MetricsDto
{
    public ulong CountTotal { get; set; }
    public ulong CountLast12Hours { get; set; }
    public ulong CountLast24Hours { get; set; }
    public ulong CountLast72Hours { get; set; }
}