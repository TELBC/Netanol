using System.Buffers.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DotNetFlow.Netflow9;
using Fennec.Services;
using Serilog;
using Serilog.Context;

namespace Fennec.TraceImporters;

public class NetFlow9TraceImporter : BackgroundService
{
    private readonly UdpClient _udpClient;
    private readonly ILogger _log;
    private readonly IServiceProvider _serviceProvider;

    public NetFlow9TraceImporter(ILogger log, IServiceProvider serviceProvider)
    {
        _log = log.ForContext<NetFlow9TraceImporter>();
        _udpClient = new UdpClient(2055);
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var result = await _udpClient.ReceiveAsync(ct);
            using var idCtx = LogContext.PushProperty("TraceGuid", Guid.NewGuid());
            _log.ForContext("TrafficBytes", result.Buffer)
                .Debug("Received {TraceImporterType} bytes from {TraceExporterIp} " +
                         "with a length of {PacketLength} bytes", 
                    TraceImporterType.Netflow9,
                    result.RemoteEndPoint.ToString(), 
                    result.Buffer.Length);

            var info = ReadSingleTrace(result);
            if (info == null)
                continue;

            var scope = _serviceProvider.CreateScope();
            var importer = scope.ServiceProvider.GetRequiredService<ITraceImportService>();
            importer.ImportTrace(info);
        }
    }

    private TraceImportInfo? ReadSingleTrace(UdpReceiveResult result)
    {
        try
        {
            var stream = new MemoryStream(result.Buffer);
            using var nr = new NetflowReader(stream);

            _ = nr.ReadPacketHeader();
            var template = nr.ReadFlowSet() as TemplateFlowSet;
            var data = nr.ReadFlowSet() as DataFlowSet;
            var view = new NetflowView(data, template);
            var record = view[0];

            // TODO: read the correct information here
            return new TraceImportInfo(
                DateTimeOffset.Now, IPAddress.Loopback,
                record.IPv4SourceAddress, 0,
                record.IPv4DestinationAddress, 0,
                0,
                0);
        }
        catch (Exception ex)
        {
            _log.ForContext("Base64Bytes", Convert.ToBase64String(result.Buffer))
                .Error(ex, "Failed to parse bytes to {TraceImporterType}", TraceImporterType.Netflow9);
            return null;
        }
    }
}