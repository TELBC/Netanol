using System.Diagnostics;
using System.Net.Sockets;
using Fennec.Collectors;
using Fennec.Database;
using Fennec.Options;
using Fennec.Parsers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog.Context;

namespace Fennec.Services;

/// <summary>
/// A service that listens for UDP packets on a specified port and forwards them
/// to the appropriate parser based on the protocol version.
/// </summary>
public class ProtocolMultiplexerService : BackgroundService
{
    private readonly ILogger _log;
    private readonly ITraceRepository _traceRepository;
    private readonly UdpClient _udpClient;
    private readonly IDictionary<ParserType, IParser> _parsers;
    private readonly int _listeningPort;
    private readonly IWriteLatencyMetric _writeLatencyMetric;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolMultiplexerService"/>.
    /// </summary>
    /// <param name="log">Logger for logging information and errors.</param>
    /// <param name="options">Configuration options for the service.</param>
    /// <param name="parsers">The collection of data parsers.</param>
    /// <param name="listeningPort">Port the multiplexer listens on.</param>
    /// <param name="traceRepository"></param>
    public ProtocolMultiplexerService(
        ILogger log,
        IOptions<ProtocolMultiplexerOptions> options, 
        IDictionary<ParserType, IParser> parsers,
        int listeningPort, ITraceRepository traceRepository, IWriteLatencyMetric writeLatencyMetric)
    {
        _log = log.ForContext<ProtocolMultiplexerService>();
        _listeningPort = options.Value.ListeningPort;
        _parsers = parsers;
        _traceRepository = traceRepository;
        _udpClient = new UdpClient(_listeningPort);
        _writeLatencyMetric = writeLatencyMetric;
    }

    /// <summary>
    /// Executes the background service to listen for UDP packets and process them.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.Information("Multiplexer listening on port {ListeningPort}", _listeningPort);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync(stoppingToken);
                using var guidCtx = LogContext.PushProperty("TraceGuid", Guid.NewGuid());
                
                MatchPacket(result);
            }
        }
        catch (Exception ex)
        {
            _log.ForContext("Exception", ex)
                .Error("Failed to read packet due to an " +
                       "unhandled exception | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);        }
    }

    /// <summary>
    /// Matches the packet to the correct parser and calls <see cref="ReadPacket"/>.
    /// </summary>
    /// <param name="result"></param>
    private void MatchPacket(UdpReceiveResult result)
    {
        var protocolVersion = DetermineProtocolVersion(result.Buffer);
        
        foreach (var parser in _parsers)
        {
            if (protocolVersion == ProtocolVersion.Unknown) continue;
            try
            {
                // If protocol not supported it skips the ReadPacket part of code
                if ((ParserType)protocolVersion != parser.Key) continue;
                // not awaited so we can continue listening for packets
                
                var stopwatch = Stopwatch.StartNew();
                ReadPacket(parser.Value, result);
                stopwatch.Stop();
                _writeLatencyMetric.AddLatency(result.RemoteEndPoint, parser.Key, stopwatch.ElapsedMilliseconds);
                break;
            }
            catch (Exception ex)
            {
                _log.ForContext("Exception", ex)
                    .Error("Failed to read packet due to an " +
                           "unexpected exception | {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
                throw;
            }
        }
    }
    
    // ReadPacket(IParser, udpResult) --> calls parser.Parse and the result of it is written to DB
    // try/catch around ReadPacket --> since not awaited we dont know if it fails, on fail = log
    /// <summary>
    /// Parses an incoming packet using the correct parser and imports the resulting <see cref="TraceImportInfo"/>s.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="udpReceiveResult"></param>
    private async Task ReadPacket(IParser parser, UdpReceiveResult udpReceiveResult)
    {
        var traceImportInfos = parser.Parse(udpReceiveResult).ToList();
        if (!traceImportInfos.IsNullOrEmpty())
        {
            await _traceRepository.ImportTraceImportInfo(traceImportInfos);
        }
    }

    public static ProtocolMultiplexerService CreateInstance(IServiceProvider serviceProvider, IEnumerable<ParserType> parsers, int listeningPort)
    {
        var activeCollectors = new Dictionary<ParserType, IParser>();
    
        foreach (var parserType in parsers)
        {
            IParser parser = parserType switch
            {
                ParserType.Netflow9 => serviceProvider.GetRequiredService<NetFlow9Parser>(),
                ParserType.Ipfix => serviceProvider.GetRequiredService<IpFixParser>(),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            activeCollectors.Add(parserType, parser);
        }
    
        return ActivatorUtilities.CreateInstance<ProtocolMultiplexerService>(serviceProvider, activeCollectors, listeningPort);
    }

    /// <summary>
    /// Determines the protocol version of the packet based on the first two bytes.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private ProtocolVersion DetermineProtocolVersion(byte[] buffer)
    {
        if (buffer.Length < 2)
        {
            throw new ArgumentException("Buffer is too short or null");
        }

        // Read the first two bytes from the buffer as a big-endian ushort
        var version = (ushort)((buffer[0] << 8) | buffer[1]);

        return version switch
        {
            9 => ProtocolVersion.NetFlow9,
            10 => ProtocolVersion.Ipfix,
            _ => ProtocolVersion.Unknown
        };
    }
}
