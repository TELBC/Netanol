using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database;

public interface INetworkDeviceRepository
{
    Task<Dictionary<string, NetworkDevice>> GetAllNetworkDevicesAsync(CancellationToken stoppingToken);
    public void Add(NetworkDevice networkDevice);
    public Task SaveAsync(CancellationToken cancellationToken);
}

public class NetworkDeviceRepository
{
    private readonly ITapasContext _context;

    public NetworkDeviceRepository(ITapasContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, NetworkDevice>> GetAllNetworkDevicesAsync(CancellationToken stoppingToken)
    {
        return await _context.NetworkDevices.ToDictionaryAsync(d => d.DnsName, stoppingToken);
    }

    public void Add(NetworkDevice networkDevice)
    {
        _context.NetworkDevices.Add(networkDevice);
    }
    
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}