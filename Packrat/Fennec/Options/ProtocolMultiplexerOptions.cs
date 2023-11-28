using System.Collections;
using Fennec.Parsers;

namespace Fennec.Options;

/// <summary>
///    Options for the protocol multiplexer.
/// </summary>
public class ProtocolMultiplexerOptions
{
    /// <summary>
    ///     Whether the multiplexer is enabled and should be started.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     The port on which the multiplexer will listen for packets.
    /// </summary>
    public int ListeningPort { get; set; } = 2055;
    
    /// <summary>
    ///     The collectors that should be added to the multiplexer.
    /// </summary>
    public IEnumerable<ParserType> EnabledParsers { get; set; }
}