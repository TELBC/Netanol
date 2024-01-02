using System.Net.Sockets;
using Fennec.Database;
using Fennec.Metrics;
using Fennec.Options;
using Fennec.Parsers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Context;

namespace Fennec.Services;

/// <summary>
/// A service that listens for UDP packets on a specified port and forwards them
/// to the appropriate parser based on the protocol version.
/// </summary>
public class MultiplexerService
{
    private readonly ILogger _log;
    private readonly ITraceRepository _traceRepository;
    private readonly UdpClient _udpClient;
    private readonly IDictionary<ParserType, IParser> _parsers;
    private readonly MultiplexerOptions _options;
    private readonly IFlowImporterMetric _flowImporterMetric;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplexerService"/>.
    /// </summary>
    /// <param name="log">Logger for logging information and errors.</param>
    /// <param name="options">Configuration options for the service.</param>
    /// <param name="parsers">The collection of data parsers.</param>
    /// <param name="traceRepository"></param>
    public MultiplexerService(
        ILogger log,
        IDictionary<ParserType, IParser> parsers,
        MultiplexerOptions options, ITraceRepository traceRepository, IFlowImporterMetric flowImporterMetric)
    {
        _log = log.ForContext<MultiplexerService>().ForContext("MultiplexerName", options.Name);
        _options = options;
        _parsers = parsers;
        _traceRepository = traceRepository;
        _udpClient = new UdpClient(_options.ListeningPort);
        _flowImporterMetric = flowImporterMetric;
    }

    /// <summary>
    /// Executes the background service to listen for UDP packets and process them.
    /// </summary>
    public async Task RunAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _log.Information("This multiplexer is disabled");
            return;
        }
        
        _log.Information("Listening on port {MultiplexerListeningPort} for {MultiplexerParser}", _options.ListeningPort, _options.Parsers);

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
        var parserType = DetermineProtocolVersion(result.Buffer);

        if (parserType == null)
        {
            _log.Warning("Failed to determine which parser to use from initial bytes of {InitialBytes}", result.Buffer.Take(2));
            return;
        }

        if (!_parsers.TryGetValue(parserType.Value, out var parser))
        {
            _log.Warning("Received packets of type {ParserType} but this multiplexer is not configured to parse this type", parserType);
            return;
        }

        // TODO: we should keep track of this at some point
        _ = ReadPacket(parser, result);
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
            _flowImporterMetric.AddFlowImport(udpReceiveResult.RemoteEndPoint);
        }
    }

    public static MultiplexerService CreateInstance(IServiceProvider serviceProvider, MultiplexerOptions options)
    {
        var activeCollectors = new Dictionary<ParserType, IParser>();
    
        foreach (var parserType in options.Parsers)
        {
            IParser parser = parserType switch
            {
                ParserType.Netflow9 => ActivatorUtilities.CreateInstance<NetFlow9Parser>(serviceProvider),
                ParserType.Ipfix => ActivatorUtilities.CreateInstance<IpFixParser>(serviceProvider),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            activeCollectors.Add(parserType, parser);
        }
    
        return ActivatorUtilities.CreateInstance<MultiplexerService>(serviceProvider, activeCollectors, options);
    }

    /// <summary>
    /// Determines the protocol version of the packet based on the first two bytes.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static ParserType? DetermineProtocolVersion(IReadOnlyList<byte> buffer)
    {
        if (buffer.Count < 2)
        {
            throw new ArgumentException("Buffer is too short or null");
        }

        // Read the first two bytes from the buffer as a big-endian ushort
        var version = (ushort)((buffer[0] << 8) | buffer[1]);

        return version switch
        {
            9 => ParserType.Netflow9,
            10 => ParserType.Ipfix,
            _ => null
        };
    }
}
