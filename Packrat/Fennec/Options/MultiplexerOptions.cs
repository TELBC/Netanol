using Fennec.Parsers;

namespace Fennec.Options;

/// <summary>
///    Options for the protocol multiplexer.
/// </summary>
public class MultiplexerOptions
{
    /// <summary>
    ///     Whether the multiplexer is enabled and should be started.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     A name for the multiplexer used during logging.
    /// </summary>
    public string? Name { get; set; } = null;
    
    /// <summary>
    ///     The port on which the multiplexer will listen for packets.
    /// </summary>
    public int ListeningPort { get; set; } = 2055;

    /// <summary>
    ///     The collectors that should be added to the multiplexer.
    /// </summary>
    public IEnumerable<FlowProtocol> Parsers { get; set; } = new List<FlowProtocol>();
}