using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;
using Fennec.Database;
using Fennec.Options;
using Fennec.Services;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace Fennec.Collectors;

/// <summary>
/// Collector for NetFlow v9 packets.
/// </summary>
public class NetFlow9Collector : ICollector
{
    private readonly ILogger _log;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDictionary<(IPAddress, ushort), TemplateRecord> _templateRecords; // matches (ExporterIp, TemplateId) to TemplateRecord
    private readonly IMetricService _metricService;
    
    public NetFlow9Collector(ILogger log, IServiceProvider serviceProvider, IMetricService metricService)
    {
        _log = log.ForContext<NetFlow9Collector>();
        _serviceProvider = serviceProvider;
        _templateRecords = new Dictionary<(IPAddress, ushort), TemplateRecord>();
        _metricService = metricService;
    }
    public IEnumerable<TraceImportInfo> Parse(ICollector collector, UdpReceiveResult result)
    {
        var stream = new MemoryStream(result.Buffer);
        using var nr = new NetflowReader(stream, 0, _templateRecords.Values);
        var header = nr.ReadPacketHeader();

        for (var i = 0; i < header.Count; i++)
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
                            _log.Warning("[Netflow9Collector] Could not parse data set... " +
                                         "Reading this set requires a not yet transmitted " +
                                         "template set with id #{TemplateSetId}", set.ID);
                            continue;
                        }

                        var view = new NetflowView(dataFlowSet, template);
                        return CreateTraceImportInfoList(view, result);
                    case TemplateFlowSet templateFlowSet:
                        foreach (var templateRecord in templateFlowSet.Records)
                        {
                            _templateRecords.Add((result.RemoteEndPoint.Address, templateRecord.ID), templateRecord);
                            _log.Information("[Netflow9Collector] Received new template set with id #{TemplateSetId}", templateRecord.ID);
                        }

                        break;
                    case OptionsTemplateFlowSet:
                        _log.Verbose("[Netflow9Collector] OptionsTemplateFlowSet does not contain flow relevant data -> Skipping");
                        break;
                    case OptionsDataFlowSet:
                        _log.Verbose("[Netflow9Collector] OptionsDataFlowSet does not contain flow relevant data -> Skipping");
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                _log.Verbose("[Netflow9Collector] Reached end of packet");
                break;
            }
            catch (FormatException ex)
            {
                _log.ForContext("Exception", ex)
                    .ForContext("PacketBytes", result.Buffer)
                    .Warning("[Netflow9Collector] Could not parse the packet... It is apparently " +
                             "wrongly formatted | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
            }
            catch (Exception ex)
            {
                _log.ForContext("Exception", ex)
                    .Error("[Netflow9Collector] Failed to extract data from the packet due to an " +
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
            _log.Verbose("[Netflow9Collector] Read single trace | {@SingleTraceInfo}",
                new { Source = $"{info.SrcIp}:{info.SrcPort}", 
                    Destination = $"{info.DstIp}:{info.DstPort}", 
                    info.PacketCount, info.ByteCount });
            var metrics = _metricService.GetMetrics<CollectorSingleTraceMetrics>("Netflow9Metrics");
            metrics.PacketCount++;
            metrics.ByteCount += (ulong) result.Buffer.Length;
        }

        return traceImportInfos;
    }

    public TraceImportInfo CreateTraceImportInfo(dynamic record, UdpReceiveResult result)
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
    public ProtocolVersion DetermineProtocolVersion(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 2)
        {
            throw new ArgumentException("Buffer is too short or null.");
        }

        // Read the first two bytes from the buffer as a big-endian ushort
        ushort version = (ushort)((buffer[0] << 8) | buffer[1]);

        switch (version)
        {
            case 9:
                return ProtocolVersion.NetFlow9;
            default:
                return ProtocolVersion.Unknown;
        }
    }
}