using System.Net;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Serilog.Context;

namespace Fennec.Services;

/// <summary>
/// Imports traces into the database. Responsible for preventing serial and parallel duplication.
/// </summary>
public interface ITraceImportService
{
    /// <summary>
    /// Starts a new task to import the trace and returns immediately.
    /// </summary>
    /// <param name="info"></param>
    public void ImportTraceSync(TraceImportInfo info);

    /// <summary>
    /// Imports the trace and blocks until the trace is imported.
    /// </summary>
    public Task ImportTraceAsync(TraceImportInfo info);
}

public record TraceImportInfo(
    DateTimeOffset ReadTime, IPAddress ExporterIp,
    IPAddress SrcIp, ushort SrcPort,
    IPAddress DstIp, ushort DstPort,
    ulong PacketCount, ulong ByteCount);

public class TraceImportService : ITraceImportService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _log;

    public TraceImportService(IServiceProvider serviceProvider, ILogger log)
    {
        _serviceProvider = serviceProvider;
        _log = log.ForContext<TraceImportService>();
    }

    // TODO: alex handle database writing in same task
    public void ImportTraceSync(TraceImportInfo info)
    {
        _ = Task.Run(async () => await TryImportTraceAsync(info));
    }

    private async Task TryImportTraceAsync(TraceImportInfo info)
    {
        try
        {
            await ImportTraceAsync(info);
            _log.Verbose("Successfully imported trace");
        }
        catch (Exception e)
        {
            _log.ForContext("Exception", e)
                .Error("Failed to import trace due to unhandled exception | {ExceptionName}: {ExceptionMessage}",
                    e.GetType().Name, e.Message);
        }
    }

    public async Task ImportTraceAsync(TraceImportInfo info)
    {
        using var scope = _serviceProvider.CreateScope();
        var traceRepository = scope.ServiceProvider.GetRequiredService<ITraceRepository>();
        
        var srcHost = await traceRepository.GetNetworkHost(info.SrcIp);
        var dstHost = await traceRepository.GetNetworkHost(info.DstIp);

        await traceRepository.AddSingleTrace(
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