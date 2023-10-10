using System.Net;
using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database;

/// <summary>
///     Database operation abstractions for handling traces.
/// </summary>
public interface ITraceRepository
{
    /// <summary>
    ///     Add a single trace to the database.
    /// </summary>
    /// <param name="singleTrace"></param>
    /// <returns></returns>
    public Task AddSingleTrace(SingleTrace singleTrace);

    /// <summary>
    /// Get or create a <see cref="NetworkHost"/> by its <see cref="IPAddress"/>.
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public Task<NetworkHost> GetNetworkHost(IPAddress ipAddress);
}

public class TraceRepository : ITraceRepository
{
    private readonly IPackratContext _context;

    public TraceRepository(IPackratContext context)
    {
        _context = context;
    }

    public async Task AddSingleTrace(SingleTrace singleTrace)
    {
        _context.SingleTraces.Add(singleTrace);
        await _context.SaveChangesAsync();
    }

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