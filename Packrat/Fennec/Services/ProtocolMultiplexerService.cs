using System.Net.Sockets;
using Fennec.Collectors;
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
    private readonly ILogger<ProtocolMultiplexerService> _log;
    private readonly UdpClient _udpClient;
    private readonly IEnumerable<ICollector> _collectors;
    private readonly int _listeningPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolMultiplexerService"/>.
    /// </summary>
    /// <param name="log">Logger for logging information and errors.</param>
    /// <param name="options">Configuration options for the service.</param>
    /// <param name="collectors">The collection of data collectors.</param>
    public ProtocolMultiplexerService(
        ILogger<ProtocolMultiplexerService> log,
        IOptions<ProtocolMultiplexerOptions> options, 
        IEnumerable<ICollector> collectors)
    {
        _log = log;
        _listeningPort = options.Value.ListeningPort;
        _collectors = collectors;
        _udpClient = new UdpClient(_listeningPort);
    }

    /// <summary>
    /// Executes the background service to listen for UDP packets and process them.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Multiplexer listening on port {_listeningPort}", _listeningPort);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync(stoppingToken);
                using var guidCtx = LogContext.PushProperty("TraceGuid", Guid.NewGuid());

                foreach (var collector in _collectors)
                {
                    var protocolVersion = collector.DetermineProtocolVersion(result.Buffer);
                    if (protocolVersion == ProtocolVersion.Unknown) continue;
                    collector.ReadSingleTraces(result);
                    break; // Assuming only one collector will process a given packet
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An error occurred in the ProtocolMultiplexerService");
        }
    }
}
