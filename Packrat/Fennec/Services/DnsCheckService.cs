using Fennec.Database;
using Fennec.Options;
using Microsoft.Extensions.Options;

namespace Fennec.Services;

public class DnsCheckService : BackgroundService
{
    private readonly ITraceRepository _traceRepository;
    private readonly DnsResolverService _dnsResolverService;
    private readonly ILogger _log;
    private readonly TimeSpan _checkInterval;

    public DnsCheckService(ITraceRepository traceRepository, DnsResolverService dnsResolverService,
        ILogger logger, IOptions<DnsCheckServiceOptions> options)
    {
        _traceRepository = traceRepository;
        _dnsResolverService = dnsResolverService;
        _log = logger;
        _checkInterval = options.Value.CheckInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _log.Information("Checking DNS records at UTC: {Time}", DateTimeOffset.UtcNow);

            var traces = await _traceRepository.GetAllTraces();

            foreach (var trace in traces)
            {
                if (!string.IsNullOrEmpty(trace.Source.DnsName) &&
                    !string.IsNullOrEmpty(trace.Destination.DnsName)) continue;
                var srcDnsEntry = _dnsResolverService.GetDnsEntryFromCacheOrResolve(trace.Source.IpAddress);
                var dstDnsEntry = _dnsResolverService.GetDnsEntryFromCacheOrResolve(trace.Destination.IpAddress);

                if (!string.IsNullOrEmpty(srcDnsEntry.Result.HostName))
                {
                    trace.Source.DnsName = srcDnsEntry.Result.HostName;
                }

                if (!string.IsNullOrEmpty(dstDnsEntry.Result.HostName))
                {
                    trace.Destination.DnsName = dstDnsEntry.Result.HostName;
                }

                await _traceRepository.UpdateTrace(trace);
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    public static DnsCheckService CreateInstance(IServiceProvider serviceProvider, TimeSpan checkInterval)
    {
        var traceRepository = serviceProvider.GetRequiredService<ITraceRepository>();
        var dnsResolverService = serviceProvider.GetRequiredService<DnsResolverService>();
        var log = serviceProvider.GetRequiredService<ILogger>();
        return ActivatorUtilities.CreateInstance<DnsCheckService>(serviceProvider, traceRepository, dnsResolverService, log,
            checkInterval);
    }
}