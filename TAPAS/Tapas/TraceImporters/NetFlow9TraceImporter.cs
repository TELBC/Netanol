using System.Dynamic;
using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;
using Microsoft.AspNetCore.Identity;
using Microsoft.CSharp.RuntimeBinder;
using Tapas.Database;
using Tapas.Models;

namespace Tapas.Listeners;

public class NetFlow9TraceImporter : BackgroundService
{
    private readonly UdpClient _udpClient;
    private readonly TraceRepository _traceRepository;

    public NetFlow9TraceImporter(TraceRepository traceRepository)
    {
        _udpClient = new UdpClient(22055);
        _traceRepository = traceRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var result = await _udpClient.ReceiveAsync(ct);
            var singleTrace = ReadSingleTrace(result);
            await _traceRepository.AddSingleTrace(singleTrace);
        }
    } 
    
    private static bool DoesPropertyExist(dynamic settings, string name)
    {
        if (settings is ExpandoObject)
            return ((IDictionary<string, object>)settings).ContainsKey(name);

        return settings.GetType().GetProperty(name) != null;
    }
    
    
    
    private SingleTrace ReadSingleTrace(UdpReceiveResult result)
    {
        var stream = new MemoryStream(result.Buffer);
        using var nr = new NetflowReader(stream);

        _ = nr.ReadPacketHeader();
        var template = nr.ReadFlowSet() as TemplateFlowSet;
        var data = nr.ReadFlowSet() as DataFlowSet;
        var view = new NetflowView(data, template);
        var record = view[0]; 
        var timestamp = DateTimeOffset.UtcNow;
        try 
        {
            var check = record.IPv4SourceAddress;
            return new SingleTrace(TraceProtocol.Udp, IPAddress.Loopback, record.IPv4SourceAddress,
                (int)record.Layer4SourcePort, record.IPv4DestinationAddress,
                (int)record.Layer4DestinationPort, timestamp);
        }
        catch (RuntimeBinderException)
        {
            return new SingleTrace(TraceProtocol.Udp, IPAddress.Loopback, record.IPv6SourceAddress,
                (int)record.Layer4SourcePort, record.IPv6DestinationAddress,
                (int)record.Layer4DestinationPort, timestamp);
        }
    }
}
