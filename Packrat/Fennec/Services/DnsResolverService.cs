using System.Net;
using System.Net.Sockets;
using Fennec.Database;
using Fennec.Options;
using Microsoft.Extensions.Options;
using SQLitePCL;

namespace Fennec.Services;

/// <summary>
/// Service that resolves IP addresses to DNS entries.
/// </summary>
public class DnsResolverService
{
    private readonly ILogger _log;
    private Dictionary<IPAddress, (string?, DateTime)> _dnsCache = new();
    private readonly TimeSpan _invalidationDuration;

    public DnsResolverService(ILogger log, IOptions<DnsCacheOptions> options)
    {
        _log = log;
        _invalidationDuration = options.Value.InvalidationDuration;
    }

    /// <summary>
    /// Resolves an IP address to a DNS entry.
    /// </summary>
    /// <param name="ipAddress"></param>
    private async Task<string?> ResolveIpToDns(IPAddress ipAddress)
    {
        try
        {
            var ipHostEntry = await Dns.GetHostEntryAsync(ipAddress);
            return ipHostEntry.HostName;
        }
        catch (SocketException e)
        {
            _log.Verbose("Could not resolve {IpAddress} to DNS entry: {Message}", ipAddress, e.Message);
        }
        catch (Exception e)
        {
            _log.Error("Unexpected exception while resolving {IpAddress} to DNS entry: {Message}", ipAddress,
                e.Message);
        }

        return null;
    }

    /// <summary>
    /// Resolves an IP address to a DNS entry, either from the cache or by calling <see cref="ResolveIpToDns"/>.
    /// </summary>
    /// <param name="ipAddress">IP to get from _dnsCache or resolve</param>
    /// <returns></returns>
    public async Task<string?> GetDnsEntryFromCacheOrResolve(IPAddress ipAddress)
    {
        if (!_dnsCache.TryGetValue(ipAddress, out var cachedDns)) // if IP not in cache
        {
            var hostname = await ResolveIpToDns(ipAddress);
            if (hostname != null)
            {
                _dnsCache.Add(ipAddress, (hostname, DateTime.Now));
            }
            else // if IP not in cache and could not be resolved
            {
                _dnsCache.Add(ipAddress, (null, DateTime.Now));
                _log.Verbose(
                    "No DNS Entry for {IpAddress} was found continuing with empty value. Check every {DnsInvalidationDuration}.",
                    ipAddress, _invalidationDuration);
            }

            return hostname;
        }

        if (DateTime.Now - cachedDns.Item2 <= _invalidationDuration) // if IP in cache and not expired
        {
            return cachedDns.Item1;
        }

        return await ResolveIpToDns(ipAddress);
    }

    /// <summary>
    /// Called by <see cref="DnsCacheCleanupService"/> to clean up the DNS cache.
    /// </summary>
    public void CleanupDnsCache()
    {
        var cutoff = DateTime.Now.Subtract(_invalidationDuration); // remove entries older than InvalidationDuration
        _dnsCache = _dnsCache
            .Where(kvp => kvp.Value.Item2 > cutoff)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
        IOptions<DnsCacheOptions> options)
    {
        _dnsResolverService = dnsResolverService;
        _log = log;
        _cleanupInterval = options.Value.CleanupInterval;
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
}