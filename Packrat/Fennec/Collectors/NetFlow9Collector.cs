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

            _log.ForContext("TrafficBytes", result.Buffer)
                .Debug("Received {FlowCollectorType} bytes from {TraceExporterIp} " +
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
                        var template = _templateRecords[key];

                        var view = new NetflowView(dataFlowSet, template);
                        WriteSingleTrace(view, result);

                        _log.ForContext("Netflow9Collector", CollectorType.Netflow9)
                            .Verbose("Writing Trace with Collector {CollectorType}; Sequence ID {SequenceID}.", CollectorType.Netflow9, header.SequenceNumber);

                        break;
                    case TemplateFlowSet templateFlowSet:
                        foreach (var templateRecord in templateFlowSet.Records)
                        {
                            _templateRecords.Add((result.RemoteEndPoint.Address, templateRecord.ID), templateRecord);
                            _log.Information("Added TemplateFlowSet {TemplateRecordId}.",
                                (result.RemoteEndPoint.Address, templateRecord.ID));
                        }

                        break;
                    case OptionsTemplateFlowSet optionsTemplateFlowSet:
                        _log.ForContext("OptionsTemplateFlowSet", optionsTemplateFlowSet)
                            .Debug("OptionsTemplateFlowSet does not contain flow relevant data. Skipping...");
                        break;
                    case OptionsDataFlowSet optionsDataFlowSet:
                        _log.ForContext("OptionsDataFlowSet", optionsDataFlowSet)
                            .Debug("OptionsDataFlowSet does not contain flow relevant data. Skipping...");
                        break;
                }
            }
            catch (EndOfStreamException ex)
            {
                _log.ForContext("EndOfStreamException", ex)
                    .Debug(
                        "Reached end of stream while Reading {FlowCollectorType} bytes from {TraceExporterIp} with a length of {PacketLength} bytes",
                        CollectorType.Netflow9, result.RemoteEndPoint.ToString(), result.Buffer.Length);
                break;
            }
            catch (KeyNotFoundException ex)
            {
                _log.ForContext("KeyNotFoundException", ex)
                    .Warning(
                        "Could not find template record for data set in packet with sequence number {SequenceNumber}",
                        header.SequenceNumber);
            }
            catch (FormatException ex)
            {
                _log.ForContext("FormatException", ex)
                    .Error("The input data are corrupt and cannot be processed. The flow set has been skipped.");
            }
            catch (InvalidOperationException)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.ForContext("Unknown Exception", ex)
                    .Error("Unknown exception while reading Netflow9 packet.");
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
            importer.ImportTraceSync(info);
            _log.ForContext("TraceImportInfo", info)
                .Verbose("Writing Trace for trace {TraceImportInfo}.", info);
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