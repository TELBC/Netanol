using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;
using Fennec.Options;
using Fennec.Services;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace Fennec.Collectors;

public class NetFlow9Collector : BackgroundService
{
    private readonly ILogger _log;
    private readonly Netflow9CollectorOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly UdpClient _udpClient;

    public NetFlow9Collector(ILogger log, IOptions<Netflow9CollectorOptions> iOptions,
        IServiceProvider serviceProvider)
    {
        _options = iOptions.Value;

        _log = log.ForContext<NetFlow9Collector>();
        _udpClient = new UdpClient(_options.ListeningPort);
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            _log.Information("Netflow9 collector is disabled... Rerun the application to enable it");
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            var result = await _udpClient.ReceiveAsync(ct);
            using var guidCtx = LogContext.PushProperty("TraceGuid", Guid.NewGuid());

            _log.ForContext("TrafficBytes", result.Buffer)
                .Debug("Received {FlowCollectorType} bytes from {TraceExporterIp} " +
                       "with a length of {PacketLength} bytes",
                    CollectorType.Netflow9,
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
            _log.Error(ex, "Failed to parse bytes to {FlowCollectorType}", CollectorType.Netflow9);
            return null;
        }
    }
}