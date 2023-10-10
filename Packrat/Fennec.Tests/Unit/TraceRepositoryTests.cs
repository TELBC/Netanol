using System.Net;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Tests.Unit;

public class TraceRepositoryTests
{
    private readonly IPackratContext _context;
    private readonly TraceRepository _repository;

    public TraceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PackratContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique name ensures a new database for each test
            .Options;

        _context = new PackratContext(options);
        _repository = new TraceRepository(_context);
    }

    [Fact]
    public async Task AddSingleTrace_Test()
    {
        var singleTrace = new SingleTrace(
            IPAddress.Broadcast,
            DateTimeOffset.UtcNow,
            TraceProtocol.Udp,
            new NetworkHost(IPAddress.Loopback),
            0,
            new NetworkHost(IPAddress.Loopback),
            0,
            2,
            0);

        await _repository.AddSingleTrace(singleTrace);

        var result =
            _context.SingleTraces.FirstOrDefault(st =>
                st.Id == singleTrace.Id); // Assuming SingleTrace has an Id property
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetNetworkHost_ReturnsExistingHost()
    {
        var ipAddress = IPAddress.Parse("127.0.0.1");
        var networkHost = new NetworkHost(ipAddress);
        _context.NetworkHosts.Add(networkHost);
        await _context.SaveChangesAsync();

        var result = await _repository.GetNetworkHost(ipAddress);

        Assert.NotNull(result);
        Assert.Equal(ipAddress, result.IpAddress);
    }

    [Fact]
    public async Task GetNetworkHost_ReturnsNewHost()
    {
        var ipAddress = IPAddress.Parse("192.168.0.1");

        var result = await _repository.GetNetworkHost(ipAddress);

        var savedHost = _context.NetworkHosts.FirstOrDefault(nh => nh.IpAddress.Equals(ipAddress));
        Assert.NotNull(savedHost);
        Assert.Equal(ipAddress, result.IpAddress);
    }
}