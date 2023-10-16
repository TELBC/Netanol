namespace Fennec.Options;

public class IpfixCollectorOptions
{
    /// <summary>
    ///     Whether the collector is enabled and should be started.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     The port on which the collector will listen for Ipfix packets.
    /// </summary>
    public short ListeningPort { get; set; } = 2055;
}