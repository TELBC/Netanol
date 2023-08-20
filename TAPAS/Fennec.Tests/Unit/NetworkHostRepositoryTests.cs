using Moq;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Fennec.Tests.Unit;

public class NetworkHostRepositoryTests
{
    private readonly ITapasContext _context;
    private readonly NetworkHostRepository _repository;

    public NetworkHostRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TapasContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TapasContext(options);
        _repository = new NetworkHostRepository(_context);
    }

    [Fact]
    public async Task GetNetworkHostsToUpdateAsync_Test()
    {
        var networkHost1 = new NetworkHost(IPAddress.Parse("2.2.2.2"));
        var networkHost2 = new NetworkHost(IPAddress.Parse("3.3.3.3"));
        
        _repository.Add(networkHost1);
        _repository.Add(networkHost2);
        await _context.SaveChangesAsync();

        var results = await _repository.GetNetworkHostsToUpdateAsync(CancellationToken.None);

        // Asserts
        Assert.NotNull(results);
        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task SaveAsync_Test()
    {
        var ip = IPAddress.Parse("1.1.1.1");
        _repository.Add(new NetworkHost(ip));
        await _repository.SaveAsync(CancellationToken.None);
        
        var savedHost = _context.NetworkHosts.FirstOrDefault(h => h.IpAddress.Equals(ip));

        // Asserts
        Assert.NotNull(savedHost);
        Assert.Equal(ip, savedHost.IpAddress);
    }

    [Fact]
    public void Add_Test()
    {
        var ip = IPAddress.Parse("8.8.8.8");
        var host = new NetworkHost(ip);
        _repository.Add(host);
        _context.SaveChangesAsync();

        var savedHost = _context.NetworkHosts.FirstOrDefault(h => h.IpAddress.Equals(IPAddress.Parse("8.8.8.8")));
        
        // Asserts
        Assert.NotNull(savedHost);
        Assert.Equal(ip, savedHost.IpAddress);
    }
}