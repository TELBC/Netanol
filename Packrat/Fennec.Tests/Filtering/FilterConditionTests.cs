using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Database.Domain.Layers;

namespace Fennec.Tests.Filtering;

public class FilterConditionTests
{
    private readonly AggregateTrace _trace = new(
        sourceIpBytes: new byte[] { 192, 168, 1, 100 },
        destinationIpBytes: new byte[] { 10, 0, 0, 1 },
        sourcePort: 12345,
        destinationPort: 80,
        protocol: TraceProtocol.Tcp,
        packetCount: 10,
        byteCount: 1000
    );

    private readonly AggregateTrace _trace2 = new(
        sourceIpBytes: new byte[] { 192, 168, 1, 100 },
        destinationIpBytes: new byte[] { 10, 0, 0, 1 },
        sourcePort: 12345,
        destinationPort: 80,
        protocol: TraceProtocol.Tcp,
        packetCount: 10,
        byteCount: 1000
    );
    
    [Fact]
    public void MatchWithExactAddressesPortsAndProtocol()
    {
        // Arrange
        var filter = new FilterCondition(
            sourceAddress: new byte[] { 192, 168, 1, 100 },
            sourceAddressMask: new byte[] { 255, 255, 255, 255 },
            sourcePort: 12345,
            destinationAddress: new byte[] { 10, 0, 0, 1 },
            destinationAddressMask: new byte[] { 255, 255, 255, 255 },
            destinationPort: 80,
            protocol: TraceProtocol.Tcp,
            include: true
        );

        var trace = new AggregateTrace(
            sourceIpBytes: new byte[] { 192, 168, 1, 100 },
            destinationIpBytes: new byte[] { 10, 0, 0, 1 },
            sourcePort: 12345 ,
            destinationPort: 80,
            protocol: TraceProtocol.Tcp,
            packetCount: 1000,
            byteCount: 10
        );

        // Act
        var result = filter.MatchesAggregateTrace(trace);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MismatchOnExactSourceAddress()
    {
        // Arrange
        var filter = new FilterCondition(
            sourceAddress: new byte[] { 192, 168, 1, 101 }, // Intentionally incorrect
            sourceAddressMask: new byte[] { 255, 255, 255, 255 },
            sourcePort: 12345,
            destinationAddress: new byte[] { 10, 0, 0, 1 },
            destinationAddressMask: new byte[] { 255, 255, 255, 255 },
            destinationPort: 80,
            protocol: TraceProtocol.Tcp,
            include: true
        );

        // Act
        var result = filter.MatchesAggregateTrace(_trace);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void MatchWithVagueAddressesExactPortAndProtocol()
    {
        // Arrange
        var filter = new FilterCondition(
            sourceAddress: new byte[] { 192, 168, 1, 0 }, // Matched by mask
            sourceAddressMask: new byte[] { 255, 255, 255, 0 },
            sourcePort: 12345,
            destinationAddress: new byte[] { 10, 0, 0, 0 }, // Matched by mask
            destinationAddressMask: new byte[] { 255, 255, 255, 0 },
            destinationPort: 80,
            protocol: TraceProtocol.Tcp,
            include: true
        );

        // Act
        var result = filter.MatchesAggregateTrace(_trace2);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void MatchWithWildCardAddressExactPortAndProtocol()
    {
        // Arrange
        var filter = new FilterCondition(
            sourceAddress: new byte[] { 0, 0, 0, 0 }, // Ignored due to mask
            sourceAddressMask: new byte[] { 0, 0, 0, 0 },
            sourcePort: 12345,
            destinationAddress: new byte[] { 0, 0, 0, 0 }, // Ignored due to mask
            destinationAddressMask: new byte[] { 0, 0, 0, 0 },
            destinationPort: 80,
            protocol: TraceProtocol.Tcp,
            include: true
        );

        // Act
        var result = filter.MatchesAggregateTrace(_trace);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void MismatchWithProtocolAndExactAddressAndPort()
    {
        // Arrange
        var filter = new FilterCondition(
            sourceAddress: new byte[] { 192, 168, 1, 100 },
            sourceAddressMask: new byte[] { 255, 255, 255, 255 },
            sourcePort: 12345,
            destinationAddress: new byte[] { 10, 0, 0, 1 },
            destinationAddressMask: new byte[] { 255, 255, 255, 255 },
            destinationPort: 80,
            protocol: TraceProtocol.Udp, // Intentional mismatch
            include: true
        );

        // Act
        var result = filter.MatchesAggregateTrace(_trace2);

        // Assert
        Assert.False(result);
    }
}