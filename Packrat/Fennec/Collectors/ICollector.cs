using System.Net.Sockets;
using Fennec.Database;
using Fennec.Services;

namespace Fennec.Collectors;

/// <summary>
/// Interface for all collectors.
/// </summary>
public interface ICollector
{
    IEnumerable<TraceImportInfo> Parse(ICollector collector, UdpReceiveResult result);
}