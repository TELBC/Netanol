using System.Net;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Parsers;
using Fennec.Utils;

namespace Fennec.Tests.Integration;

public class QueryConditionTests : MongoDbFactory
{
    private async Task SeedDatabase()
    {
        var traceCollection = Database.GetCollection<SingleTrace>("singleTraces");
        var traces = new SingleTrace[]
        {
            new(DateTimeOffset.Now, DataProtocol.Udp, FlowProtocol.Ipfix, false,
                new SingleTraceEndpoint(IPAddress.Parse("1.1.1.1"), 20),
                new SingleTraceEndpoint(IPAddress.Parse("1.1.1.2"), 30), 100, 20),
            new(DateTimeOffset.Now, DataProtocol.Tcp, FlowProtocol.Netflow9, true,
                new SingleTraceEndpoint(IPAddress.Parse("2.1.1.1"), 20),
                new SingleTraceEndpoint(IPAddress.Parse("2.1.1.2"), 30), 100, 20),
            new(DateTimeOffset.Now, DataProtocol.Tcp, FlowProtocol.Netflow5, false,
                new SingleTraceEndpoint(IPAddress.Parse("3.1.1.1"), 10),
                new SingleTraceEndpoint(IPAddress.Parse("3.1.1.2"), 50), 100, 20)
        };

        await traceCollection.InsertManyAsync(traces);
    }

    [Fact]
    public async Task FiltersByDuplicates()
    {
        // Arrange
        await SeedDatabase();
        var conditions = new QueryConditions
        {
            AllowDuplicates = false
        };

        var service = new TraceRepository(Database, null!, null!);

        // Act
        var traces = await service.AggregateTraces(conditions, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        // Assert
        Assert.True(traces.Count == 2);
        // Assert.Contains(IPAddress.Parse("1.1.1.1").GetAddressBytes(), first.SourceIpBytes);
    }

    [Fact]
    public async Task FiltersByPort()
    {
        // Arrange
        await SeedDatabase();
        var conditions = new QueryConditions
        {
            FlowProtocolsWhitelist = new[] { FlowProtocol.Ipfix, FlowProtocol.Netflow5 },
            DataProtocolsWhitelist = new[] { DataProtocol.Tcp }
        };

        var service = new TraceRepository(Database, null!, null!);

        // Act
        var traces = await service.AggregateTraces(conditions, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        // Assert
        Assert.True(traces.Count == 1);
        var first = traces.First();
        Assert.Equal(IPAddress.Parse("3.1.1.1").GetAddressBytes(), first.SourceIpBytes);
    }

    [Fact]
    public async Task FiltersByDuplicateAndPort()
    {
        // Arrange
        await SeedDatabase();
        var conditions = new QueryConditions
        {
            AllowDuplicates = false,
            PortsWhitelist = new[] { 10, 50 }
        };

        var service = new TraceRepository(Database, null!, null!);

        // Act
        var traces = await service.AggregateTraces(conditions, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        // Assert
        Assert.True(traces.Count == 1);
        var first = traces.First();
        Assert.Equal(IPAddress.Parse("3.1.1.1").GetAddressBytes(), first.SourceIpBytes);
    }
}