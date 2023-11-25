using System.Net.Sockets;
using Fennec.Collectors;
using Fennec.Database;
using Fennec.Options;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace Fennec.Services;

/// <summary>
/// A service that listens for UDP packets on a specified port and forwards them
/// to the appropriate collector based on the protocol version.
/// </summary>
public class ProtocolMultiplexerService : BackgroundService
{
    private readonly ILogger _log;
    private readonly ITraceRepository _traceRepository;
    private readonly UdpClient _udpClient;
    private readonly IDictionary<CollectorType, ICollector> _collectors;
    private readonly int _listeningPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolMultiplexerService"/>.
    /// </summary>
    /// <param name="log">Logger for logging information and errors.</param>
    /// <param name="options">Configuration options for the service.</param>
    /// <param name="collectors">The collection of data collectors.</param>
    /// <param name="listeningPort">Port the multiplexer listens on.</param>
    /// <param name="traceRepository"></param>
    public ProtocolMultiplexerService(
        ILogger log,
        IOptions<ProtocolMultiplexerOptions> options, 
        IDictionary<CollectorType, ICollector> collectors,
        int listeningPort, ITraceRepository traceRepository)
    {
        _log = log.ForContext<ProtocolMultiplexerService>();
        _listeningPort = options.Value.ListeningPort;
        _collectors = collectors;
        _traceRepository = traceRepository;
        _udpClient = new UdpClient(_listeningPort);
    }

    /// <summary>
    /// Executes the background service to listen for UDP packets and process them.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.Information("[ProtocolMultiplexer] Multiplexer listening on port {ListeningPort}", _listeningPort);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync(stoppingToken);
                using var guidCtx = LogContext.PushProperty("TraceGuid", Guid.NewGuid());

                var protocolVersion = DetermineProtocolVersion(result.Buffer);

                // only 1 collector in _collectors will ever match the protocol version per packet
                foreach (var collector in _collectors)
                {
                    if (protocolVersion == ProtocolVersion.Unknown) continue;
                    try
                    {
                        if ((CollectorType)protocolVersion == collector.Key)
                        {
                            // not awaited so we can continue listening for packets
                            ReadPacket(collector.Value, result);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error("[ProtocolMultiplexer] Packet of Type {ProtocolVersion} could not be parsed by {CollectorType}", protocolVersion, collector.Key);
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error("[ProtocolMultiplexer] Error while listening for UDP packets: {Message}", ex.Message);
        }
    }
    
    // ReadPacket(ICollector, udpResult) --> calls collector.Parse and the result of it is written to DB
    // try/catch around ReadPacket --> since not awaited we dont know if it fails, on fail = log
    /// <summary>
    /// Parses an incoming packet using the correct collector and imports the resulting <see cref="TraceImportInfo"/>s.
    /// </summary>
    /// <param name="collector"></param>
    /// <param name="udpReceiveResult"></param>
    private async Task ReadPacket(ICollector collector, UdpReceiveResult udpReceiveResult)
    {
        var traceImportInfos = collector.Parse(collector, udpReceiveResult);
        await _traceRepository.ImportTraceImportInfo(traceImportInfos);
    }

    public static ProtocolMultiplexerService CreateInstance(IServiceProvider serviceProvider, IEnumerable<CollectorType> collectors, int listeningPort)
    {
        var activeCollectors = new Dictionary<CollectorType, ICollector>();
    
        foreach (var collectorType in collectors)
        {
            ICollector collector = collectorType switch
            {
                CollectorType.Netflow9 => serviceProvider.GetRequiredService<NetFlow9Collector>(),
                CollectorType.Ipfix => serviceProvider.GetRequiredService<IpFixCollector>(),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            activeCollectors.Add(collectorType, collector);
        }
    
        return ActivatorUtilities.CreateInstance<ProtocolMultiplexerService>(serviceProvider, activeCollectors, listeningPort);
    }
    
    /// <summary>
    /// Factory method to register collectors.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="collectorTypes"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void RegisterCollectors(IServiceCollection services, IEnumerable<CollectorType> collectorTypes)
    {
        foreach (var collectorType in collectorTypes)
        {
            switch (collectorType)
            {
                case CollectorType.Netflow9:
                    services.AddSingleton<NetFlow9Collector>();
                    break;
                case CollectorType.Ipfix:
                    services.AddSingleton<IpFixCollector>();
                    break;
                // Add cases for other collector types
                default:
                    throw new ArgumentOutOfRangeException(nameof(collectorType), collectorType, null);
            }
        }
    }

    /// <summary>
    /// Determines the protocol version of the packet based on the first two bytes.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private ProtocolVersion DetermineProtocolVersion(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 2)
        {
            throw new ArgumentException("Buffer is too short or null");
        }

        // Read the first two bytes from the buffer as a big-endian ushort
        ushort version = (ushort)((buffer[0] << 8) | buffer[1]);

        return version switch
        {
            9 => ProtocolVersion.NetFlow9,
            10 => ProtocolVersion.Ipfix,
            _ => ProtocolVersion.Unknown
        };
    }
}
