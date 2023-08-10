using System.Net;
using Microsoft.EntityFrameworkCore;
using Fennec.Controllers;
using Fennec.Database.Domain.Technical;

namespace Fennec.Database;

public record HostCommunicationKey(NetworkHost SourceHost, NetworkHost DestinationHost);

public class TraceRepository
{
    private readonly TapasContext _context;

    public TraceRepository(TapasContext context)
    {
        _context = context;
    }

    public async Task AddSingleTrace(SingleTrace singleTrace)
    {
        _context.SingleTraces.Add(singleTrace);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get or create a <see cref="NetworkHost"/> by its <see cref="IPAddress"/>.
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="device"></param>
    /// <returns></returns>
    public async Task<NetworkHost> GetNetworkHost(IPAddress ipAddress, NetworkDevice device)
    {
        var host = await _context.NetworkHosts
            .Where(n => n.IpAddress == ipAddress)
            .FirstOrDefaultAsync();

        if (host != null)
            return host;

        host = new NetworkHost(ipAddress, device);
        _context.NetworkHosts.Add(host);
        await _context.SaveChangesAsync();
        return host;
    }
    public async Task<NetworkDevice> GetNetworkDevice(string dnsName)
    {
        var device = await _context.NetworkDevices
            .Where(d => d.DnsName == dnsName)
            .FirstOrDefaultAsync();

        if (device != null)
            return device;

        device = new NetworkDevice(dnsName);
        _context.NetworkDevices.Add(device);
        await _context.SaveChangesAsync();
        return device;
    }
}