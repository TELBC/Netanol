using System.Net.Sockets;
using Fennec.Database;

namespace Fennec.Parsers;

/// <summary>
/// Interface for all parsers.
/// </summary>
public interface IParser
{
    IEnumerable<TraceImportInfo> Parse(UdpReceiveResult result);
}