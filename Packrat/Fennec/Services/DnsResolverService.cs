using System.Net;
using System.Net.Sockets;
using Fennec.Database;
using Fennec.Options;
using Microsoft.Extensions.Options;

namespace Fennec.Services;

/// <summary>
/// Service that resolves IP addresses to DNS entries.
/// </summary>
public class DnsResolverService
{
    private readonly ILogger _log;
    private Dictionary<IPAddress, Tuple<IPHostEntry, DateTime>> _dnsCache = new();
    private readonly TimeSpan _staleEntryDuration;

    public DnsResolverService(ILogger log, TimeSpan staleEntryDuration)
    {
        _log = log;
        _staleEntryDuration = staleEntryDuration;
    }

    /// <summary>
    /// Resolves an IP address to a DNS entry.
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    private IPHostEntry ResolveIpToDns(IPAddress ipAddress)
    {
        try
        {
            return Dns.GetHostEntry(ipAddress);
        }
        catch (SocketException ex)
        {
            _log.Debug($"Could not resolve IP {ipAddress} to DNS: {ex.Message}");
            return new IPHostEntry(); // return empty entry
        }
        catch (Exception ex)
        {
            _log.Error("Unexpected exception while resolving IP to DNS: " + ex.Message);
            return new IPHostEntry(); // return empty entry
        }
    }

    /// <summary>
    /// Resolves an IP address to a DNS entry, either from the cache or by calling <see cref="ResolveIpToDns"/>.
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public async Task<IPHostEntry> GetDnsEntryFromCacheOrResolve(IPAddress ipAddress)
    {
        if (!_dnsCache.TryGetValue(ipAddress, out var cachedDns))
            return await Task.Run(() => ResolveIpToDns(ipAddress));
        if ((DateTime.Now - cachedDns.Item2).TotalMinutes <= 60) // cache valid for 60 minutes
        {
            return cachedDns.Item1;
        }

        return await Task.Run(() => ResolveIpToDns(ipAddress));
    }

    /// <summary>
    /// Called by <see cref="DnsCacheCleanupService"/> to clean up the DNS cache.
    /// </summary>
    public void CleanupDnsCache()
    {
        var cutoff = DateTime.Now.Subtract(_staleEntryDuration); // remove entries older than StaleEntryDuration
        _dnsCache = _dnsCache
            .Where(kvp => kvp.Value.Item2 > cutoff)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    public static DnsResolverService CreateInstance(IServiceProvider serviceProvider, TimeSpan staleEntryDuration)
    {
        var log = serviceProvider.GetRequiredService<ILogger>();
        return ActivatorUtilities.CreateInstance<DnsResolverService>(serviceProvider, log, staleEntryDuration);
    }
}

/// <summary>
/// Background service that cleans up the DNS cache.
/// </summary>
public class DnsCacheCleanupService : BackgroundService
{
    private readonly DnsResolverService _dnsResolverService;
    private readonly ILogger _log;
    private readonly TimeSpan _cleanupInterval;

    public DnsCacheCleanupService(DnsResolverService dnsResolverService, ILogger log,
        IOptions<DnsCheckServiceOptions> options)
    {
        _dnsResolverService = dnsResolverService;
        _log = log;
        _cleanupInterval = options.Value.CheckInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _log.Information("Running DnsCacheCleanupService.");
            _dnsResolverService.CleanupDnsCache();
            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    public static DnsCacheCleanupService CreateInstance(IServiceProvider serviceProvider, TimeSpan cleanupInterval)
    {
        var dnsResolverService = serviceProvider.GetRequiredService<DnsResolverService>();
        var log = serviceProvider.GetRequiredService<ILogger>();
        return ActivatorUtilities.CreateInstance<DnsCacheCleanupService>(serviceProvider, dnsResolverService, log,
            cleanupInterval);
    }
}