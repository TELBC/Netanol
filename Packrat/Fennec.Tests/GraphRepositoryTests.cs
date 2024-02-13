using System.Net;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Database.Domain.Layers;
using Fennec.Database.Graph;
using NSubstitute;

namespace Fennec.Tests;

public class GraphRepositoryTests
{
    private static ITraceRepository GetMockTraceRepository(List<AggregateTrace> sampleTraces)
    {
        var mockTraceRepository = Substitute.For<ITraceRepository>();
        mockTraceRepository
            .AggregateTraces(Arg.Any<QueryConditions>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(Task.FromResult(sampleTraces));
        return mockTraceRepository;
    }
    
    [Fact]
    public async Task GenerateGraph_WithEmptyLayout_ShouldProcessCorrectly()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow;
        var emptyLayout = new Layout("") { Layers = new List<ILayer>() };

        var sampleTraces = new List<AggregateTrace>
        {
            new()
            {
                SourceIpBytes = IPAddress.Parse("192.168.1.1").GetAddressBytes(),
                SourcePort = 80,
                DestinationIpBytes = IPAddress.Parse("10.0.0.1").GetAddressBytes(),
                DestinationPort = 5603,
                Protocol = DataProtocol.Tcp,
                PacketCount = 100,
                ByteCount = 2000
            },
            new()
            {
                SourceIpBytes = IPAddress.Parse("192.168.1.2").GetAddressBytes(),
                SourcePort = 120,
                DestinationIpBytes = IPAddress.Parse("10.0.0.2").GetAddressBytes(),
                DestinationPort = 3293,
                Protocol = DataProtocol.Tcp,
                PacketCount = 150,
                ByteCount = 3000
            }
        };

        var traceRepository = GetMockTraceRepository(sampleTraces);
        var graphRepository = new GraphRepository(traceRepository);

        // Act
        var result = await graphRepository.GenerateGraph(from, to, emptyLayout);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.TotalHostCount);
        Assert.Equal(5000, result.TotalByteCount);
        Assert.Equal(250, result.TotalPacketCount);
        Assert.Equal(2, result.TotalTraceCount);
        Assert.Equal(4, result.Nodes.Count); // Assuming each IP results in a unique node
        Assert.Equal(2, result.Edges.Count); // Assuming one edge per trace
    }

    [Fact]
    public async Task GenerateGraph_WithEmptyLayout_ShouldCollapseCorrectly()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow;
        var emptyLayout = new Layout("") { Layers = new List<ILayer>() };
        
        var sampleTraces = new List<AggregateTrace>
        {
            new()
            {
                SourceIpBytes = IPAddress.Parse("192.168.1.1").GetAddressBytes(),
                SourcePort = 80,
                DestinationIpBytes = IPAddress.Parse("10.0.0.1").GetAddressBytes(),
                DestinationPort = 5603,
                Protocol = DataProtocol.Udp,
                PacketCount = 10,
                ByteCount = 1000
            },
            new()
            {
                SourceIpBytes = IPAddress.Parse("192.168.1.1").GetAddressBytes(),
                SourcePort = 120,
                DestinationIpBytes = IPAddress.Parse("10.0.0.1").GetAddressBytes(),
                DestinationPort = 3293,
                Protocol = DataProtocol.Tcp,
                PacketCount = 50,
                ByteCount = 1000
            }
        };
        
        var traceRepository = GetMockTraceRepository(sampleTraces);
        var graphRepository = new GraphRepository(traceRepository);
        
        // Act
        var result = await graphRepository.GenerateGraph(from, to, emptyLayout);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalHostCount);
        Assert.Equal(2000, result.TotalByteCount);
        Assert.Equal(60, result.TotalPacketCount);
        Assert.Equal(1, result.TotalTraceCount);
        Assert.Equal(2, result.Nodes.Count); 
        Assert.Single(result.Edges); 
    }
}