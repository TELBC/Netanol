using Moq;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Tests.Unit;

public class NetworkDeviceRepositoryTests
{
    private readonly ITapasContext _context;
    private readonly NetworkDeviceRepository _repository;

    public NetworkDeviceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TapasContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TapasContext(options);
        _repository = new NetworkDeviceRepository(_context);
    }

    [Fact]
    public async Task GetAllNetworkDevicesAsync_ReturnsAllDevices()
    {
        var networkDevices = new List<NetworkDevice>
        {
            new NetworkDevice("test1.local"),
            new NetworkDevice("test2.local"),
            new NetworkDevice("test3.local")
        };
        await _context.NetworkDevices.AddRangeAsync(networkDevices);
        await _repository.SaveAsync();

        var results = await _repository.GetAllNetworkDevicesAsync(CancellationToken.None);

        // Asserts
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);

        // Assert that the devices are sorted by DNS name
        var devicesList = results.Values.ToList();
        Assert.Equal("test2.local", devicesList[1].DnsName);
    }

    [Fact]
    public async Task SaveAsync_Test()
    {
        var device = new NetworkDevice("test.local");
        _repository.Add(device);
        await _repository.SaveAsync(CancellationToken.None);

        var savedDevice = _context.NetworkDevices.FirstOrDefault(d => d.DnsName == "test.local");

        // Asserts
        Assert.NotNull(savedDevice);
        Assert.Equal("test.local", savedDevice.DnsName);
    }

    [Fact]
    public async void Add_AddsNetworkDevice()
    {
        var device = new NetworkDevice("test.local");
        _repository.Add(device);
        await _repository.SaveAsync();

        var savedDevice = _context.NetworkDevices.FirstOrDefault(d => d.DnsName == "test.local");
        
        // Asserts
        Assert.NotNull(savedDevice);
    }
}