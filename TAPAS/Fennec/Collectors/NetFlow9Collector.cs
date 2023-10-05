using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;
using Fennec.Options;
using Fennec.Services;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace Fennec.Collectors;

//
//                 |
//                 |
//                 |
//                 |
// ----------------------------------
//                 |
//                 |
//                 |
//                 |
//                 |
//                 |
//                 |
//                 |
//                 |
//                 |
//                 |
//                 |
//

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
            var header = nr.ReadPacketHeader();
            
            var allFlowSets = new List<object>();
            var onlyDataFlowSets = new List<DataFlowSet>();

            // header.Count stores how many FlowSets are contained inside a packet.
            for (var i = 0; i < header.Count; i++)
            {
                try
                {
                    var flowSet = nr.ReadFlowSet(_allTemplateRecords);
                    allFlowSets.Add(flowSet);
                }
                catch
                {
                    _log.Error($"Could not read/add FlowSet at index {i} in packet with sequence number {header.SequenceNumber}");
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
                else if (flowSet is DataFlowSet dataFlowSet) // check what FlowSet is a DataFlowSet (isn't always the case)
                {
                    onlyDataFlowSets.Add(dataFlowSet);
                }
                else
                {
                    // TODO : Decide what to do with non-network data FlowSets (OptionsTemplateFlowSet, OptionsDataFlowSet)
                    _log.Error($"Dropping non-network data FlowSet of type {flowSet.GetType().Name}");
                    return null;
                }
            }

            NetflowView? view = null;
            foreach (var dataFlowSet in onlyDataFlowSets)
            {
                view = new NetflowView(dataFlowSet, _allTemplateRecords);
            }
            
            var record = view[0];

            if (record != null)
            {
                // any property you call on record has to be present and therefore match the template it was parsed with
                // TODO: getting the exporter IP not possible since this is not sent with NetFlow
                return new TraceImportInfo(
                    DateTimeOffset.UtcNow, IPAddress.Loopback,
                    record.IPv4SourceAddress, (int)record.Layer4SourcePort,
                    record.IPv4DestinationAddress, Math.Abs((int)record.Layer4DestinationPort),
                    (int)record.IncomingPackets,
                    (int)record.IncomingBytes);
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