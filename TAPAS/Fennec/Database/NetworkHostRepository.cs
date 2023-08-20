using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database;

public interface INetworkHostRepository
{
    Task<IEnumerable<NetworkHost>> GetNetworkHostsToUpdateAsync(CancellationToken stoppingToken);
    public Task SaveAsync(CancellationToken cancellationToken);
    public void Add(NetworkHost networkHost);
    
}

public class NetworkHostRepository : INetworkHostRepository
{
    private readonly ITapasContext _context;

    public NetworkHostRepository(ITapasContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NetworkHost>> GetNetworkHostsToUpdateAsync(CancellationToken stoppingToken)
    {
        var cutOffTime = DateTimeOffset.UtcNow.AddDays(-1);
        return await _context.NetworkHosts
            .Include(h => h.DnsInformation)
            .Where(h => h.DnsInformation == null || h.DnsInformation.LastAccessedDnsName <= cutOffTime)
            .ToListAsync(stoppingToken);
    }
    
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public void Add(NetworkHost networkHost)
    {
        _context.NetworkHosts.Add(networkHost);
    }
}