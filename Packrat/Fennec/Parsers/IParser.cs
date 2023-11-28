using System.Net.Sockets;
using Fennec.Database;

namespace Fennec.Parsers;

/// <summary>
/// Interface for all collectors.
/// </summary>
public interface IParser
{
    IEnumerable<TraceImportInfo> Parse(UdpReceiveResult result);
}