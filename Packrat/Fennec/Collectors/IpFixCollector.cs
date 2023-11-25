using System.Collections;
using System.Net;
using System.Net.Sockets;
using DotNetFlow.Ipfix;
using Fennec.Database;
using Fennec.Services;
using FormatException = System.FormatException;
using TemplateRecord = DotNetFlow.Ipfix.TemplateRecord;

namespace Fennec.Collectors;

/// <summary>
/// Collector for IPFIX packets.
/// </summary>
public class IpFixCollector : ICollector
{
    private readonly ILogger _log;
    private readonly IServiceProvider _serviceProvider;
    // TODO: expand _templateRecords to a service, can be used to display/monitor templates in frontend
    private readonly IDictionary<(IPAddress, ushort), TemplateRecord> _templateRecords;
    private readonly IMetricService _metricService;

    public IpFixCollector(ILogger log, IServiceProvider serviceProvider, IMetricService metricService)
    {
        _log = log.ForContext<IpFixCollector>();
        _serviceProvider = serviceProvider;
        _templateRecords = new Dictionary<(IPAddress, ushort), TemplateRecord>();
        _metricService = metricService;
    }

    public IEnumerable<TraceImportInfo> Parse(ICollector collector, UdpReceiveResult result)
    {
        // result.RemoteEndPoint.Address --> address of exporter
        var stream = new MemoryStream(result.Buffer);

        using var ipfixReader = new IpfixReader(stream, 0, _templateRecords.Values);
        var header = ipfixReader.ReadPacketHeader();

        // Process each set in the IPFIX message. Has to be while(true) because header doesn't provide a count of sets.
        while (true)
        {
            try
            {
                var set = ipfixReader.ReadFlowSet();

                switch (set)
                {
                    case DataSet dataSet:
                        var key = (result.RemoteEndPoint.Address, set.ID);
                        if (!_templateRecords.TryGetValue(key, out var template))
                        {
                            _log.Warning("[IpFixCollector] Could not parse data set... " +
                                         "Reading this set requires a not yet transmitted " +
                                         "template set with id #{TemplateSetId}", set.ID);
                            continue;
                        }

                        var view = new IpfixView(dataSet, template);
                        return CreateTraceImportInfoList(view, result);
                    case TemplateSet templateSet:
                        foreach (var templateRecord in templateSet.Records)
                        {
                            _templateRecords.Add((result.RemoteEndPoint.Address, templateRecord.ID), templateRecord);
                            _log.Information("[IpFixCollector] Received new template set with id #{TemplateSetId}", templateRecord.ID);
                        }

                        break;
                }
            }
            catch (EndOfStreamException) // TODO: include logs for specific Exceptions that we know the meaning of + increase context
            {
                _log.Verbose("[IpFixCollector] Reached end of packet");
                break;
            }
            catch (FormatException ex)
            {
            
                _log.ForContext("Exception", ex)
                    .ForContext("PacketBytes", result.Buffer)
                    .Warning("[IpFixCollector] Could not parse the packet... It is apparently " +
                             "wrongly formatted | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
            }
            catch (Exception ex)
            {
                _log.ForContext("Exception", ex)
                    .Error("[IpFixCollector] Failed to extract data from the packet due to an " +
                           "unexpected exception | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
            }
        }
        
        return Enumerable.Empty<TraceImportInfo>();
    }

    private IEnumerable<TraceImportInfo> CreateTraceImportInfoList(IpfixView view, UdpReceiveResult result)
    {
        var traceImportInfos = new List<TraceImportInfo>();
        for (var i = 0; i < view.Count; i++)
        {
            var info = CreateTraceImportInfo(view[i], result);
            traceImportInfos.Add(info);
            _log.Verbose("[IpFixCollector] Read single trace | {@SingleTraceInfo}",
                new { Source = $"{info.SrcIp}:{info.SrcPort}", 
                    Destination = $"{info.DstIp}:{info.DstPort}", 
                    info.PacketCount, info.ByteCount });
            var metrics = _metricService.GetMetrics<CollectorSingleTraceMetrics>("IpFixMetrics");
            metrics.PacketCount++;
            metrics.ByteCount += (ulong) result.Buffer.Length;
        }

        return traceImportInfos;
    }

    private TraceImportInfo CreateTraceImportInfo(dynamic record, UdpReceiveResult result)
    {
        // TODO: change readTime to flow duration or include both maybe --> more info for frontend
        var properties = (IDictionary<string, object>)record;
        var readTime = DateTimeOffset.UtcNow; // TODO: handle flows with ex. 2 packets total duration 0.000000000 ms
        var exporterIp = result.RemoteEndPoint.Address;
        
        // Yes! These double casts are necessary. Don't ask me why.
        var srcIp = properties.TryGetValue("SourceIPv4Address", out var property) ? (IPAddress) property : IPAddress.None;
        var srcPort = properties.TryGetValue("SourceTransportPort", out var property1) ? property1 : (ushort) 0;
        var dstIp = properties.TryGetValue("DestinationIPv4Address", out var property2) ? (IPAddress) property2 : IPAddress.None;
        var dstPort = properties.TryGetValue("DestinationTransportPort", out var property3) ? property3 : (ushort) 0;
        var packetCount = properties.TryGetValue("PacketDeltaCount", out var property4) ? property4 : (ulong) 0;
        var byteCount = properties.TryGetValue("OctetDeltaCount", out var property5) ? property5 : (ulong) 0;
    
        return new TraceImportInfo(
            readTime, exporterIp,
            srcIp, (ushort) srcPort,
            dstIp, (ushort) dstPort,
            (ulong) packetCount, (ulong) byteCount
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
            case 10:
                return ProtocolVersion.Ipfix;
            default:
                return ProtocolVersion.Unknown;
        }
    }
}