using System.Net;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Fennec.Services;
using Moq;

namespace Fennec.Tests.Unit;

public class TraceImportServiceTests
{
    private readonly Mock<ITraceRepository> _mockTraceRepository;
    private readonly ITraceImportService _service;

    public TraceImportServiceTests()
    {
        _mockTraceRepository = new Mock<ITraceRepository>();
        _service = new TraceImportService(_mockTraceRepository.Object);
    }

    [Fact]
    public async Task ImportTrace_Test()
    {
        var info = new TraceImportInfo(
            DateTimeOffset.UtcNow, IPAddress.Parse("10.0.0.1"),
            IPAddress.Parse("192.168.0.1"), 12345,
            IPAddress.Parse("192.168.0.2"), 54321,
            10, 1000
        );

        var networkHost1 = new NetworkHost(IPAddress.Parse("192.168.0.1"));
        var networkHost2 = new NetworkHost(IPAddress.Parse("192.168.0.2"));

        _mockTraceRepository.Setup(repo =>
                repo.GetNetworkHost(It.Is<IPAddress>(ip => ip.Equals(IPAddress.Parse("192.168.0.1")))))
            .ReturnsAsync(networkHost1);

        _mockTraceRepository.Setup(repo =>
                repo.GetNetworkHost(It.Is<IPAddress>(ip => ip.Equals(IPAddress.Parse("192.168.0.2")))))
            .ReturnsAsync(networkHost2);

        _service.ImportTrace(info);
        await Task.Delay(100);

        _mockTraceRepository.Verify(repo => repo.GetNetworkHost(It.IsAny<IPAddress>()), Times.Exactly(2));
        _mockTraceRepository.Verify(repo => repo.AddSingleTrace(It.IsAny<SingleTrace>()), Times.Once);
    }
}