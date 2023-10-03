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
    private readonly List<TemplateRecord> _allTemplateRecords;

    public NetFlow9Collector(ILogger log, IOptions<Netflow9CollectorOptions> iOptions,
        IServiceProvider serviceProvider)
    {
        _options = iOptions.Value;

        _log = log.ForContext<NetFlow9Collector>();
        _udpClient = new UdpClient(_options.ListeningPort);
        _serviceProvider = serviceProvider;
        _allTemplateRecords = new List<TemplateRecord>();
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

            // How to carry-over Templates through packets 
            
            var header = nr.ReadPacketHeader();
            var allFlowSets = new List<object>();
            var onlyDataFlowSets = new List<DataFlowSet>();

            for (var i = 0; i < header.Count; i++)
            {
                try
                {
                    var flowSet = nr.ReadFlowSet(_allTemplateRecords);
                    allFlowSets.Add(flowSet);
                }
                catch (Exception e)
                {
                    // ignored :)
                }
            }

            foreach (var flowSet in allFlowSets)
            {
                if (flowSet is TemplateFlowSet templateFlowSet)
                {
                    foreach (var templateRecord in templateFlowSet.Records)
                    {
                        if (!_allTemplateRecords.Contains(templateRecord))
                        {
                            _allTemplateRecords.Add(templateRecord);
                            _log.Information($"Template Record added {templateRecord.ID}");
                        }
                    }
                }
                else if (flowSet is DataFlowSet dataFlowSet)
                {
                    onlyDataFlowSets.Add(dataFlowSet);
                }
                else
                {
                    _log.Error($"Dropping packet OptionsDataFlowSet/OptionsTemplateFlowSet");
                    return null;
                }
                // TODO : Decide what to do with non-network data FlowSets (OptionsTemplateFlowSet, OptionsDataFlowSet)
            }

            NetflowView? view = null;
            foreach (var dataFlowSet in onlyDataFlowSets)
            {
                view = new NetflowView(dataFlowSet, _allTemplateRecords);
            }
            
            var record = view[0];

            // TODO: read the correct information here
            if (record != null)
            {
                return new TraceImportInfo(
                    DateTimeOffset.Now, IPAddress.Loopback,
                    record.IPv4SourceAddress, 0,
                    record.IPv4DestinationAddress, 0,
                    0,
                    0);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse bytes to {FlowCollectorType}", CollectorType.Netflow9);
            return null;
        }
    }
}