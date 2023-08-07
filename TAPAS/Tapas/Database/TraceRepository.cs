using System.Net;
using Microsoft.EntityFrameworkCore;
using Tapas.Controllers;
using Tapas.Models;

namespace Tapas.Database;

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
    /// <returns></returns>
    public async Task<NetworkHost> GetNetworkHost(IPAddress ipAddress)
    {
        var host = await _context.NetworkHosts
            .Where(n => n.IpAddress == ipAddress)
            .FirstOrDefaultAsync();

        if (host != null)
            return host;

        host = new NetworkHost(ipAddress);
        _context.NetworkHosts.Add(host);
        await _context.SaveChangesAsync();
        return host;
    }
}