using System.Net;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Parsers;

namespace Fennec.Tests.Integration;

public class TraceRepositoryTests : MongoDbFactory
{
    
    private static readonly IPAddress Ip1 = IPAddress.Parse("1.1.1.1");
    private static readonly IPAddress Ip2 = IPAddress.Parse("2.2.2.2");
    
    private readonly List<SingleTrace> _singleTraces = new()
    {
        new SingleTrace
        {
            Source = new SingleTraceEndpoint(Ip1, 0, "vhost1.local"),
            Destination = new SingleTraceEndpoint(Ip2, 0, "vhost2.local"),
            ByteCount = 1000,
            PacketCount = 100,
            DataProtocol = DataProtocol.Tcp,
            FlowProtocol = FlowProtocol.Ipfix,
            Duplicate = false,
            Timestamp = DateTime.Now - TimeSpan.FromHours(1)
        },

        new SingleTrace
        {
            Source = new SingleTraceEndpoint(Ip1, 0, "dnsServer.local"),
            Destination = new SingleTraceEndpoint(Ip2, 0, "vhost1.local"),
            ByteCount = 1000,
            PacketCount = 100,
            DataProtocol = DataProtocol.Tcp,
            FlowProtocol = FlowProtocol.Ipfix,
            Duplicate = false,
            Timestamp = DateTime.Now
        }
    };

    private async Task SeedDatabase()
    {
        var singleTraces = Database.GetCollection<SingleTrace>("singleTraces");
        await singleTraces.InsertManyAsync(_singleTraces);
    }

    [Fact]
    public async Task CorrectlyAggregatesTraces()
    {
        // Arrange
        await SeedDatabase();
        var service = new TraceRepository(Database, null!, null!);

        // Act
        var traces = await service.AggregateTraces(
            new QueryConditions(), DateTime.MinValue, DateTime.MaxValue);

        // Assert
        Assert.True(traces.Count == 1);
        Assert.Equal("dnsServer.local", traces[0].SourceDnsName);
    }
}