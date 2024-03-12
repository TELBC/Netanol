using System.Net;
using System.Net.Sockets;
using Fennec.Options;
using Microsoft.Extensions.Options;

namespace Fennec.Services;

/// <summary>
///     Service that resolves IP addresses to DNS entries.
/// </summary>
public interface IDnsResolverService
{
    /// <summary>
    ///     Resolves an IP address to a DNS entry, returns null if no entry is found.
    /// </summary>
    /// <param name="ipAddress">IP to get from _dnsCache or resolve</param>
    /// <returns></returns>
    Task<string?> GetDnsEntryFromCacheOrResolve(IPAddress ipAddress);

    /// <summary>
    /// Remove entries older than the <see cref="DnsCacheOptions.InvalidationDuration"/> from the cache.
    /// </summary>
    public void CleanupDnsCache();

    /// <summary>
    ///     Returns the number of cached entries.
    /// </summary>
    public int CachedEntriesCount { get; }
    
    public DateTimeOffset LastCleanup { get; } 
}

public class DnsResolverService : IDnsResolverService
{
    private readonly ILogger _log;
    private readonly DnsCacheOptions _options;
    private Dictionary<IPAddress, (string? DnsName, DateTime RegistrationTime)> _dnsCache = new();
    public DateTimeOffset LastCleanup { get; private set; } = DateTimeOffset.Now;

    public int CachedEntriesCount => _dnsCache.Count;

    public DnsResolverService(ILogger log, IOptions<DnsCacheOptions> options)
    {
        _log = log.ForContext<DnsResolverService>();
        _options = options.Value;
    }

    public async Task<string?> GetDnsEntryFromCacheOrResolve(IPAddress ipAddress)
    {
        if (_dnsCache.TryGetValue(ipAddress, out var cacheEntry))
        {
            if (DateTime.Now - cacheEntry.RegistrationTime <= _options.InvalidationDuration)
                return cacheEntry.DnsName;
            
            _log.Verbose("Evicting {IpAddress} from DNS cache | {CacheEntry}", ipAddress, cacheEntry);
            _dnsCache.Remove(ipAddress);
        }
        
        var hostname = await ResolveIpToDns(ipAddress);
        _dnsCache.Add(ipAddress, (hostname, DateTime.Now));
        _log.Verbose("Resolved {IpAddress} to {DnsName}", ipAddress, hostname);

        return _dnsCache[ipAddress].DnsName;
    }

    private async Task<string?> ResolveIpToDns(IPAddress ipAddress)
    {
        try
        {
            var ipHostEntry = await Dns.GetHostEntryAsync(ipAddress);
            return ipHostEntry.HostName;
        }
        catch (Exception e)
        {
            // Prevent the console from being too cluttered with DNS resolution errors
            if (e is SocketException)
            {
                _log.Verbose(e, "Failed to resolve IP address {IpAddress} | {ExceptionName}: {ExceptionMessage}", ipAddress, 
                    e.GetType(), e.Message);
                return null;
            }
            
            _log.Error(e, "Unexpected exception while DNS resolving {IpAddress} | {ExceptionName}: {ExceptionMessage}", ipAddress,
                e.GetType(),
                e.Message);
        }

        return null;
    }

    public void CleanupDnsCache()
    {
        var cutoff = DateTime.Now.Subtract(_options.InvalidationDuration);
        
        var countBefore = _dnsCache.Count;
        _dnsCache = _dnsCache
            .Where(kvp => kvp.Value.Item2 > cutoff)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var countAfter = _dnsCache.Count;
        
        _log.Information("Cleaned up DNS cache... Before {BeforeCount} entries, removed {Difference} entries, now at {AfterCount} entries", 
            countBefore, countBefore - countAfter, countAfter);
        LastCleanup = DateTimeOffset.Now;
    }
}

/// <summary>
///     Background service that cleans up the DNS cache.
/// </summary>
public class DnsCacheCleanupService : BackgroundService
{
    private readonly TimeSpan _cleanupInterval;
    private readonly IDnsResolverService _dnsResolverService;
    private readonly ILogger _log;

    public DnsCacheCleanupService(IDnsResolverService dnsResolverService, ILogger log,
        IOptions<DnsCacheOptions> options)
    {
        _dnsResolverService = dnsResolverService;
        _log = log.ForContext<DnsCacheCleanupService>();
        _cleanupInterval = options.Value.CleanupInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _log.Information("Next DNS cache cleanup in {CleanupInterval}", _cleanupInterval);
            await Task.Delay(_cleanupInterval, stoppingToken);
            _log.Information("Cleaning up DNS cache");
            _dnsResolverService.CleanupDnsCache(); 
        }
    }
}