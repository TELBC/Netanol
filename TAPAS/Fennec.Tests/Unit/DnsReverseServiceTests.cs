using System.Net;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Fennec.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fennec.Tests.Unit
{
    public class DnsReverseServiceTests
    {
        private readonly Mock<INetworkHostRepository> _mockHostRepo;
        private readonly Mock<INetworkDeviceRepository> _mockDeviceRepo;
        private readonly DnsReverseService _service;

        public DnsReverseServiceTests()
        {
            _mockHostRepo = new Mock<INetworkHostRepository>();
            _mockDeviceRepo = new Mock<INetworkDeviceRepository>();

            var networkHosts = new List<NetworkHost>
            {
                new NetworkHost(IPAddress.Parse("1.1.1.1")),
                new NetworkHost(IPAddress.Parse("8.8.8.8")),
                new NetworkHost(IPAddress.Parse("9.9.9.9"))
            };

            _mockHostRepo.Setup(repo => repo.GetNetworkHostsToUpdateAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(networkHosts);

            // You'll want to modify this if you have some devices you want to mock for certain tests.
            _mockDeviceRepo.Setup(repo => repo.GetAllNetworkDevicesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, NetworkDevice>());

            var mockLogger = new Mock<ILogger<DnsReverseService>>();
            _service = new DnsReverseService(mockLogger.Object, _mockHostRepo.Object, _mockDeviceRepo.Object);
        }

        [Fact]
        public async Task UpdateDnsNameAsync_Test()
        {
            await _service.UpdateDnsNameAsync(CancellationToken.None);

            // Checks if DnsReverseService saved the changes to the database successfully.
            _mockHostRepo.Verify(repo => repo.SaveAsync(CancellationToken.None), Times.Once);
        }
    }
}