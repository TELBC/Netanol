using System.Net;
using System.Net.Sockets;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Services;

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
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TapasContext>();

                // TODO: make configurable?
                var cutOffTime = DateTimeOffset.UtcNow.AddDays(-1); // specifies which NetworkHosts to update (timeframe)

                var hosts = await dbContext.NetworkHosts
                    .Include(h => h.DnsInformation).Where(h =>
                        h.DnsInformation == null || h.DnsInformation.LastAccessedDnsName <= cutOffTime)
                    .ToListAsync(stoppingToken);

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

                    if (resolvedDnsName == host.DnsInformation?.DnsName) continue;
                    host.DnsInformation!.DnsName = resolvedDnsName;
                    host.DnsInformation.LastAccessedDnsName = DateTimeOffset.UtcNow;
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }

            // still have to agree on an interval -> maybe make it configurable?
            await Task.Delay(TimeSpan.FromSeconds(120), stoppingToken);
        }
    }
}