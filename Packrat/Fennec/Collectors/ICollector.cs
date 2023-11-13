using System.Net.Sockets;
using Fennec.Services;

namespace Fennec.Collectors;

/// <summary>
/// Interface for all collectors.
/// </summary>
public interface ICollector
{
    void ReadSingleTraces(UdpReceiveResult result);
    ProtocolVersion DetermineProtocolVersion(byte[] buffer);
    TraceImportInfo CreateTraceImportInfo(dynamic record, UdpReceiveResult result);
}