using System.Net;
using Fennec.Database;
using Fennec.Database.Domain.Technical;

namespace Fennec.Services;

/// <summary>
/// Imports traces into the database. Responsible for preventing serial and parallel duplication.
/// </summary>
public interface ITraceImportService
{
    /// <summary>
    /// Imports trace information.
    /// </summary>
    /// <param name="info"></param>
    public void ImportTrace(TraceImportInfo info);
}

public record TraceImportInfo(
    DateTimeOffset ReadTime, IPAddress ExporterIp,
    IPAddress SrcIp, int SrcPort,
    IPAddress DstIp, int DstPort,
    int PacketCount, int ByteCount);

public class TraceImportService : ITraceImportService
{
    private readonly ITraceRepository _traceRepository;

    public TraceImportService(ITraceRepository traceRepository)
    {
        _traceRepository = traceRepository;
    }

    public void ImportTrace(TraceImportInfo info)
    {
        _ = Task.Run(async () => await ImportTraceAsync(info));
    }

    private async Task ImportTraceAsync(TraceImportInfo info)
    {
        var srcHost = await _traceRepository.GetNetworkHost(info.SrcIp);
        var dstHost = await _traceRepository.GetNetworkHost(info.DstIp);

        await _traceRepository.AddSingleTrace(
            new SingleTrace(
                info.ExporterIp,
                info.ReadTime,
                TraceProtocol.Udp,
                srcHost,
                info.SrcPort,
                dstHost,
                info.DstPort,
                info.ByteCount,
                info.PacketCount));
    }
}