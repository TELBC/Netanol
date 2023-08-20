using System.Net;
using System.Net.Sockets;
using Fennec.Database;
using Fennec.Database.Domain.Technical;

namespace Fennec.Services;

public interface IDnsReverseService
{
    Task UpdateDnsNameAsync(CancellationToken stoppingToken);
}

public class DnsReverseService : BackgroundService, IDnsReverseService
{
    private readonly ILogger<DnsReverseService> _logger;
    private readonly INetworkHostRepository _hostRepo;
    private readonly INetworkDeviceRepository _deviceRepo;

    public DnsReverseService(ILogger<DnsReverseService> logger, INetworkHostRepository hostRepo, INetworkDeviceRepository deviceRepo)
    {
        _logger = logger;
        _hostRepo = hostRepo;
        _deviceRepo = deviceRepo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateDnsNameAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken); // TODO: make configurable
        }
    }

    public async Task UpdateDnsNameAsync(CancellationToken stoppingToken)
    {
        var hosts = await _hostRepo.GetNetworkHostsToUpdateAsync(stoppingToken);
        var networkDevices = await _deviceRepo.GetAllNetworkDevicesAsync(stoppingToken);

        foreach (var host in hosts)
        {
            string resolvedDnsName;
            try
            {
                resolvedDnsName = (await Dns.GetHostEntryAsync(host.IpAddress.ToString(), stoppingToken)).HostName;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostNotFound)
            {
                _logger.LogDebug(
                    "No DNS host found for IP address {IpAddress}. Defaulting to 'Unknown Hostname'",
                    host.IpAddress);
                resolvedDnsName = "Unknown Hostname";
            }
            catch
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
                    _deviceRepo.Add(networkDevice);
                    networkDevices[resolvedDnsName] = networkDevice;
                    host.DnsInformation = new DnsInformation(resolvedDnsName, DateTimeOffset.UtcNow, networkDevice);
                }
                else
                {
                    var networkDevice = networkDevices[resolvedDnsName];
                    host.DnsInformation = new DnsInformation(resolvedDnsName, DateTimeOffset.UtcNow, networkDevice);
                }
            }
            else if (resolvedDnsName != host.DnsInformation.DnsName)
            {
                host.DnsInformation.DnsName = resolvedDnsName;
                host.DnsInformation.LastAccessedDnsName = DateTimeOffset.UtcNow;
            }
        }

        await _hostRepo.SaveAsync(stoppingToken);
        _logger.LogInformation("DNS reverse lookup service finished updating DNS names");
    }
}