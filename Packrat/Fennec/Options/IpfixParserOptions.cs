namespace Fennec.Options;

public class IpfixParserOptions
{
    /// <summary>
    ///     Whether the collector is enabled and should be started.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    ///     The port on which the collector will listen for Ipfix packets.
    /// </summary>
    public short ListeningPort { get; set; } = 2056;
}