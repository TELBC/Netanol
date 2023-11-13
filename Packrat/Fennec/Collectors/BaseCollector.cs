using System.Net.Sockets;
using Fennec.Services;

namespace Fennec.Collectors;

/// <summary>
/// Abstract base class for all collectors.
/// </summary>
public abstract class BaseCollector : ICollector
{
    public abstract void ReadSingleTraces(UdpReceiveResult result);
    public abstract TraceImportInfo CreateTraceImportInfo(dynamic record, UdpReceiveResult result);

    /// <summary>
    /// Determines the protocol version of the given buffer.
    /// </summary>
    /// <param name="buffer"> The buffer to determine the protocol version from.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public virtual ProtocolVersion DetermineProtocolVersion(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 2)
        {
            throw new ArgumentException("Buffer is too short or null.");
        }

        // Read the first two bytes from the buffer as an ushort.
        var version = (ushort)((buffer[0] << 8) | buffer[1]);

        return version switch
        {
            9 => ProtocolVersion.NetFlow9,
            10 => ProtocolVersion.Ipfix,
            _ => ProtocolVersion.Unknown
        };
    }
}
