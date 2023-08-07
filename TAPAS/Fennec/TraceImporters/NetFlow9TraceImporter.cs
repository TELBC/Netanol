using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;
using Fennec.Services;

namespace Fennec.TraceImporters;

public class NetFlow9TraceImporter : BackgroundService
{
    private readonly UdpClient _udpClient;
    private readonly IServiceProvider _serviceProvider;

    public NetFlow9TraceImporter(IServiceProvider serviceProvider)
    {
        _udpClient = new UdpClient(22055);
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var result = await _udpClient.ReceiveAsync(ct);
            var info = ReadSingleTrace(result);
            
            var scope = _serviceProvider.CreateScope();
            var importer = scope.ServiceProvider.GetRequiredService<ITraceImportService>();
            importer.ImportTrace(info);
        }
    }

    private TraceImportInfo ReadSingleTrace(UdpReceiveResult result)
    {
        var stream = new MemoryStream(result.Buffer);
        using var nr = new NetflowReader(stream);
        
        _ = nr.ReadPacketHeader();
        var template = nr.ReadFlowSet() as TemplateFlowSet;
        var data = nr.ReadFlowSet() as DataFlowSet;
        var view = new NetflowView(data, template);
        var record = view[0];
        
        // TODO: read the correct information here
        return new TraceImportInfo(
            DateTimeOffset.Now, IPAddress.Loopback,
            record.IPv4SourceAddress, 0, 
            record.IPv4DestinationAddress, 0,
            255,
            255);
    }
}