namespace Fennec.Options;

public class Netflow9CollectorOptions
{
    /// <summary>
    ///     Whether the collector is enabled and should be started.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     The port on which the collector will listen for Netflow v9 packets.
    /// </summary>
    public short ListeningPort { get; set; } = 2055;
}