using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;
using Fennec.Options;
using Fennec.Services;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;

namespace Fennec.Collectors;

public class NetFlow9Collector : BackgroundService
{
    private readonly ILogger _log;
    private readonly Netflow9CollectorOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly UdpClient _udpClient;
    private readonly IDictionary<(IPAddress, ushort), TemplateRecord> _templateRecords; // matches (ExporterIp, TemplateId) to TemplateRecord

    public NetFlow9Collector(ILogger log, IOptions<Netflow9CollectorOptions> iOptions, IServiceProvider serviceProvider)
    {
        _options = iOptions.Value;
        _log = log.ForContext<NetFlow9Collector>();
        _udpClient = new UdpClient(_options.ListeningPort);
        _serviceProvider = serviceProvider;
        _templateRecords = new Dictionary<(IPAddress, ushort), TemplateRecord>();
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

            _log.Debug("Received {FlowCollectorType} bytes from {TraceExporterIp} " +
                       "with a length of {PacketLength} bytes",
                    CollectorType.Netflow9,
                    result.RemoteEndPoint.ToString(),
                    result.Buffer.Length);

            ReadSingleTraces(result);
        }
    }

    private void ReadSingleTraces(UdpReceiveResult result)
    {
        var stream = new MemoryStream(result.Buffer);
        using var nr = new NetflowReader(stream, 0, _templateRecords.Values);
        var header = nr.ReadPacketHeader();

        while (true)
        {
            try
            {
                var dict = _templateRecords.Values.ToDictionary(t => t.ID, t => t);
                var set = nr.ReadFlowSet(dict);

                switch (set)
                {
                    case DataFlowSet dataFlowSet:
                        var key = (result.RemoteEndPoint.Address, set.ID);
                        if (!_templateRecords.TryGetValue(key, out var template))
                        {
                            _log.Warning("Could not parse data set... " +
                                         "Reading this set requires a not yet transmitted " +
                                         "template set with id #{TemplateSetId}", set.ID);
                            continue;
                        }

                        var view = new NetflowView(dataFlowSet, template);
                        WriteSingleTrace(view, result);
                        break;
                    case TemplateFlowSet templateFlowSet:
                        foreach (var templateRecord in templateFlowSet.Records)
                        {
                            _templateRecords.Add((result.RemoteEndPoint.Address, templateRecord.ID), templateRecord);
                            _log.Information("Received new template set with id #{TemplateSetId}", templateRecord.ID);
                        }

                        break;
                    case OptionsTemplateFlowSet:
                        _log.Verbose("OptionsTemplateFlowSet does not contain flow relevant data -> Skipping");
                        break;
                    case OptionsDataFlowSet:
                        _log.Verbose("OptionsDataFlowSet does not contain flow relevant data -> Skipping");
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                _log.Verbose("Reached end of packet");
                break;
            }
            catch (FormatException ex)
            {
                _log.ForContext("Exception", ex)
                    .ForContext("PacketBytes", result.Buffer)
                    .Warning("Could not parse the packet... It is apparently " +
                             "wrongly formatted | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
            }
            catch (Exception ex)
            {
                _log.ForContext("Exception", ex)
                    .Error("Failed to extract data from the packet due to an " +
                           "unexpected exception | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
            }
        }
    }
    
    private void WriteSingleTrace(NetflowView view, UdpReceiveResult result)
    {
        var scope = _serviceProvider.CreateScope();
        var importer = scope.ServiceProvider.GetRequiredService<ITraceImportService>();

        for (var i = 0; i < view.Count; i++)
        {
            var info = CreateTraceImportInfo(view[i], result);
            _log.Verbose("Read single trace | {@SingleTraceInfo}",
                new { Source = $"{info.SrcIp}:{info.SrcPort}", 
                    Destination = $"{info.DstIp}:{info.DstPort}", 
                    info.PacketCount, info.ByteCount });
            importer.ImportTraceSync(info);
        }
    }

    private static TraceImportInfo CreateTraceImportInfo(dynamic record, UdpReceiveResult result)
    {
        var properties = (IDictionary<string, object>)record;
        var readTime = DateTimeOffset.UtcNow;
        var exporterIp = result.RemoteEndPoint.Address;

        var srcIp = properties.TryGetValue("IPv4SourceAddress", out var property)
            ? (IPAddress)property
            : IPAddress.None;
        var srcPort = properties.TryGetValue("Layer4SourcePort", out var property1)
            ? (ushort)(short)property1
            : (ushort)0;
        var dstIp = properties.TryGetValue("IPv4DestinationAddress", out var property2)
            ? (IPAddress)property2
            : IPAddress.None;
        var dstPort = properties.TryGetValue("Layer4DestinationPort", out var property3)
            ? (ushort)(short)property3
            : (ushort)0;
        var packetCount = properties.TryGetValue("IncomingPackets", out var property4) ? (ulong)(long)property4 : 0;
        var byteCount = properties.TryGetValue("IncomingBytes", out var property5) ? (ulong)(long)property5 : 0;

        return new TraceImportInfo(
            readTime, exporterIp,
            srcIp, srcPort,
            dstIp, dstPort,
            packetCount, byteCount
        );
    }
}