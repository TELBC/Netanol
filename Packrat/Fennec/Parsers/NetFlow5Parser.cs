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
    private readonly ILogger _log;

    public NetFlow5Parser(ILogger log)
    {
        _log = log.ForContext<NetFlow5Parser>();
    }

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
    /// <returns>List of <see cref="TraceImportInfo"/></returns>
    private IEnumerable<TraceImportInfo> CreateTraceImportInfoList(UdpReceiveResult result)
    {
        var importTraces = new List<TraceImportInfo>();
        var stream = new MemoryStream(result.Buffer);
        using var nr = new NetflowReader(stream);
        var header = nr.ReadPacketHeader();

        try
        {
            for (var i = 0; i < header.Count; i++)
            {
                var flow = nr.ReadFlowRecord();
                var trace = CreateTraceImportInfo(flow, result);
                importTraces.Add(trace);
            }
        }
        catch (EndOfStreamException)
        {
            _log.Verbose("Reached end of packet");
        }
        catch (InvalidOperationException ex)
        {
            _log.Error("Cannot read flow records before reading packet header. {Exception}", ex);
        }
        catch (Exception ex)
        {
            _log.ForContext("Exception", ex)
                .Error("Failed to extract data from the packet due to an " +
                       "unhandled exception | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
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