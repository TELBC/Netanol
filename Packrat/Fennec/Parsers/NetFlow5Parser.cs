using System.Net.Sockets;
using DotNetFlow.Netflow5;
using Fennec.Database;
using Fennec.Database.Domain;

namespace Fennec.Parsers;

/// <summary>
/// Parser for NetFlow v5 packets.
/// </summary>
public class NetFlow5Parser : IParser
{
    /// <summary>
    /// Parses a NetFlow v5 packet.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public IEnumerable<TraceImportInfo> Parse(UdpReceiveResult result)
    {
        var importTraces = CreateTraceImportInfoList(result);
        return importTraces;
    }
    
    /// <summary>
    /// Creates a list of <see cref="TraceImportInfo"/> from a <see cref="UdpReceiveResult"/>.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private IEnumerable<TraceImportInfo> CreateTraceImportInfoList(UdpReceiveResult result)
    {
        var stream = new MemoryStream(result.Buffer);
        using var nr = new NetflowReader(stream);
        var header = nr.ReadPacketHeader();
        
        var importTraces = new List<TraceImportInfo>();
        for (var i = 0; i < header.Count; i++)
        {
            var flow = nr.ReadFlowRecord();
            importTraces.Add(CreateTraceImportInfo(flow, result));
        }

        return importTraces;
    }

    /// <summary>
    /// Creates a <see cref="TraceImportInfo"/> from a <see cref="FlowRecord"/> and a <see cref="UdpReceiveResult"/>.
    /// </summary>
    /// <param name="flow"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private TraceImportInfo CreateTraceImportInfo(FlowRecord flow, UdpReceiveResult result)
    {
        var trace = new TraceImportInfo
        (
            DateTime.UtcNow,
            result.RemoteEndPoint.Address,
            flow.SourceAddress,
            flow.SourcePort,
            flow.DestinationAddress,
            flow.DestinationPort,
            flow.Packets,
            flow.Octets,
            (TraceProtocol)flow.Protocol
        );
        return trace;
    }
}