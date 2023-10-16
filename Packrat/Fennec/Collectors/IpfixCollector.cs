using System.Net;
using System.Net.Sockets;
using DotNetFlow.Ipfix;
using DotNetFlow.Netflow9;
using Fennec.Options;
using Fennec.Services;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using TemplateRecord = DotNetFlow.Ipfix.TemplateRecord;

namespace Fennec.Collectors;

public class IpfixCollector : BackgroundService
{
    private readonly ILogger _log;
    private readonly IpfixCollectorOptions _options;
    private readonly IServiceProvider _serviceProvider;

    private readonly UdpClient _udpClient;

    // TODO: expand to a service, can be used to display/monitor templates in frontend
    private readonly IDictionary<(IPAddress, ushort), TemplateRecord> _templateRecords;

    public IpfixCollector(ILogger log, IOptions<IpfixCollectorOptions> iOptions, IServiceProvider serviceProvider)
    {
        _options = iOptions.Value;
        _log = log.ForContext<IpfixCollector>();
        _udpClient = new UdpClient(_options.ListeningPort);
        _serviceProvider = serviceProvider;
        _templateRecords = new Dictionary<(IPAddress, ushort), TemplateRecord>();
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            _log.Information("Ipfix collector is disabled... Rerun the application to enable it");
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            var result = await _udpClient.ReceiveAsync(ct);
            using var guidCtx = LogContext.PushProperty("TraceGuid", Guid.NewGuid());

            _log.ForContext("TrafficBytes", result.Buffer)
                .Debug("Received {FlowCollectorType} bytes from {TraceExporterIp} " +
                       "with a length of {PacketLength} bytes",
                    CollectorType.Ipfix,
                    result.RemoteEndPoint.ToString(),
                    result.Buffer.Length);

            ReadSingleTraces(result);
        }
    }

    private void ReadSingleTraces(UdpReceiveResult result)
    {
        // result.RemoteEndPoint.Address --> address of exporter
        var stream = new MemoryStream(result.Buffer);

        using var ipfixReader = new IpfixReader(stream, 0, _templateRecords.Values);
        var header = ipfixReader.ReadPacketHeader();

        while (true)
        {
            try
            {
                var set = ipfixReader.ReadFlowSet();


                switch (set)
                {
                    case DataSet dataSet:
                        var key = (result.RemoteEndPoint.Address, set.ID);
                        var template = _templateRecords[key];
                        
                        var view = new IpfixView(dataSet, template);
                        WriteSingleTrace(view);
                        
                        _log.Verbose("Read {CollectorType} data with id ", CollectorType.Ipfix);
                        break;
                    case TemplateSet templateSet:

                        foreach (var templateRecord in templateSet.Records)
                        {
                            _templateRecords.Add((result.RemoteEndPoint.Address, templateRecord.ID), templateRecord);
                            _log.Information("Added TemplateSet {TemplateRecordId}",
                                (result.RemoteEndPoint.Address, templateRecord.ID));
                        }

                        break;
                }
            }
            catch (EndOfStreamException)
            {
                _log.Debug("Reached end of stream while reading IPFIX template set"); // TODO: more context information
                break;
            }
            catch (KeyNotFoundException ex) // TODO: include logs for specific Exceptions that we know the meaning of
            {
                _log.ForContext("Exception", ex)
                    .Warning("Template for parsing dataset was not found, yet to receive");
            }
            catch (Exception e)
            {
                // TODO: improve logging
                _log.Error(e.ToString());
            }
        }
    }

    private void WriteSingleTrace(IpfixView view)
    {
        var scope = _serviceProvider.CreateScope();
        var importer = scope.ServiceProvider.GetRequiredService<ITraceImportService>();
        
        for (var i = 0; i < view.Count; i++)
        {
            var info = CreateTraceImportInfo(view[i]);
            importer.ImportTrace(info);
        }
    }
    
    private static TraceImportInfo CreateTraceImportInfo(dynamic record)
    {
        var properties = (IDictionary<string, object>)record;
        var readTime = DateTimeOffset.UtcNow;
        var exporterIp = IPAddress.Loopback;
        
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
}