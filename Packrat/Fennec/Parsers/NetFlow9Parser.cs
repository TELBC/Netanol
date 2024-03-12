using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Services;

namespace Fennec.Parsers;

/// <summary>
/// Parser for NetFlow v9 packets.
/// </summary>
public class NetFlow9Parser : IParser
{
    private readonly ILogger _log;
    private readonly IMetricService _metricService;
    private readonly INetFlow9CleanupService _templateCleanupService;

    public NetFlow9Parser(ILogger log, IMetricService metricService, INetFlow9CleanupService templateCleanupService)
    {
        _log = log.ForContext<NetFlow9Parser>();
        _metricService = metricService;
        _templateCleanupService = templateCleanupService;
    }
    public IEnumerable<TraceImportInfo> Parse(UdpReceiveResult result)
    {
        var stream = new MemoryStream(result.Buffer);
        using var nr = new NetflowReader(stream, 0, _templateCleanupService.TemplateRecords.Values);
        var header = nr.ReadPacketHeader();

        for (var i = 0; i < header.Count; i++)
        {
            try
            {
                var dict = _templateCleanupService.TemplateRecords.Values.ToDictionary(t => t.ID, t => t);
                var set = nr.ReadFlowSet(dict);

                switch (set)
                {
                    case DataFlowSet dataFlowSet:
                        var key = (result.RemoteEndPoint.Address, set.ID);
                        if (!_templateCleanupService.TemplateRecords.TryGetValue(key, out var template))
                        {
                            _log.Warning("Could not parse data set... " +
                                         "Reading this set requires a not yet transmitted " +
                                         "template set with id #{TemplateSetId}", set.ID);
                            continue;
                        }

                        var view = new NetflowView(dataFlowSet, template);
                        return CreateTraceImportInfoList(view, result);
                    case TemplateFlowSet templateFlowSet:
                        foreach (var templateRecord in templateFlowSet.Records)
                        {
                            if (_templateCleanupService.TemplateRecords.ContainsKey((result.RemoteEndPoint.Address, templateRecord.ID)))
                                continue;
                            _templateCleanupService.TemplateRecords.Add((result.RemoteEndPoint.Address, templateRecord.ID), templateRecord);
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
                           "unhandled exception | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
            }
        }
        
        return Enumerable.Empty<TraceImportInfo>();
    }
    
    private IEnumerable<TraceImportInfo> CreateTraceImportInfoList(NetflowView view, UdpReceiveResult result)
    {
        var traceImportInfos = new List<TraceImportInfo>();
        for (var i = 0; i < view.Count; i++)
        {
            var info = CreateTraceImportInfo(view[i], result);
            traceImportInfos.Add(info);
            _log.Debug("Read single trace | {@SingleTraceInfo}",
                new { Source = $"{info.SrcIp}:{info.SrcPort}", 
                    Destination = $"{info.DstIp}:{info.DstPort}", 
                    info.PacketCount, info.ByteCount });
            var metrics = _metricService.GetMetrics<CollectorSingleTraceMetrics>("Netflow9Metrics");
            metrics.PacketCount++;
            metrics.ByteCount += (ulong) result.Buffer.Length;
        }

        return traceImportInfos;
    }

    private TraceImportInfo CreateTraceImportInfo(dynamic record, UdpReceiveResult result)
    {
        var properties = (IDictionary<string, object>)record;
        var readTime = DateTime.UtcNow;
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
        var protocol = properties.TryGetValue("Protocol", out var property6) ? (byte)property6 : (byte)0;
        
        return new TraceImportInfo(
            readTime, exporterIp,
            srcIp, srcPort,
            dstIp, dstPort,
            packetCount, byteCount,
            protocol switch
            {
                6 => DataProtocol.Tcp,
                17 => DataProtocol.Udp,
                _ => DataProtocol.Unknown
            },
            FlowProtocol.Netflow9
        );
    }
}