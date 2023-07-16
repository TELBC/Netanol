using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;
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

    private SingleTrace ReadSingleTrace(UdpReceiveResult result)
    {
        var stream = new MemoryStream(result.Buffer);
        using var nr = new NetflowReader(stream);
        
        _ = nr.ReadPacketHeader();
        var template = nr.ReadFlowSet() as TemplateFlowSet;
        var data = nr.ReadFlowSet() as DataFlowSet;
        var view = new NetflowView(data, template);
        var record = view[0];
        return new SingleTrace(TraceProtocol.Udp, IPAddress.Loopback, record.IPv4SourceAddress, 0, record.IPv4DestinationAddress, 0);
    }
}