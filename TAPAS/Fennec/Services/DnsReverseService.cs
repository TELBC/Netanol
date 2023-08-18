using System.Net;
using System.Net.Sockets;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Services;

/// <summary>
/// Periodically resolves DNS names for NetworkHost IP addresses and updates the database records accordingly.
/// </summary>
public class DnsReverseService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DnsReverseService> _logger;

    public DnsReverseService(IServiceProvider serviceProvider, ILogger<DnsReverseService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateDnsNameAsync(stoppingToken);
            // still have to agree on an interval TODO: make configurable
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    /// <summary>
    /// Resolves the DNS name for <see cref="NetworkHost.IpAddress"/> and updates <see cref="NetworkHost.DnsInformation"/> and creates <see cref="NetworkDevice"/> accordingly.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token to observe while waiting for the task to complete.</param>
    private async Task UpdateDnsNameAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TapasContext>();

        // specifies which NetworkHosts to update (timeframe) TODO: make configurable
        var cutOffTime =
            DateTimeOffset.UtcNow.AddDays(-1);

        // gets all NetworkHosts within the previously specified timeframe
        var hosts = await dbContext.NetworkHosts
            .Include(h => h.DnsInformation).Where(h =>
                h.DnsInformation == null || h.DnsInformation.LastAccessedDnsName <= cutOffTime)
            .ToListAsync(stoppingToken);

        // gets all NetworkDevices from the database and puts them into a dictionary for faster access
        var networkDevices = await dbContext.NetworkDevices.ToDictionaryAsync(d => d.DnsName, stoppingToken);

        foreach (var host in hosts)
        {
            string resolvedDnsName;
            try
            {
                resolvedDnsName = (await Dns.GetHostEntryAsync(host.IpAddress.ToString(), stoppingToken))
                    .HostName;
            }
            catch (SocketException)
            {
                _logger.LogDebug(
                    "No DNS host found for IP address {IpAddress}. Defaulting to 'Unknown Hostname'",
                    host.IpAddress);
                resolvedDnsName = "Unknown Hostname";
            }
            catch (Exception)
            {
                _logger.LogDebug(
                    "Unexpected error resolving IP address {IpAddress}. Defaulting to 'Unknown Hostname'",
                    host.IpAddress);
                resolvedDnsName = "Unknown Hostname";
            }

            if (host.DnsInformation == null)
            {
                if (!networkDevices.ContainsKey(resolvedDnsName))
                {
                    var networkDevice = new NetworkDevice(resolvedDnsName);
                    dbContext.NetworkDevices.Add(networkDevice);
                    networkDevices[resolvedDnsName] = networkDevice; // updates dictionary
                    host.DnsInformation =
                        new DnsInformation(resolvedDnsName, DateTimeOffset.UtcNow, networkDevice);
                }
                else
                {
                    var networkDevice = networkDevices[resolvedDnsName];
                    host.DnsInformation =
                        new DnsInformation(resolvedDnsName, DateTimeOffset.UtcNow, networkDevice);
                }
            }
            else if (resolvedDnsName != host.DnsInformation.DnsName)
            {
                host.DnsInformation.DnsName = resolvedDnsName;
                host.DnsInformation.LastAccessedDnsName = DateTimeOffset.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("DNS reverse lookup service finished updating DNS names");
    }
}