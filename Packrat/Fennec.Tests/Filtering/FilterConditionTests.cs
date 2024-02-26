using System.Net;
using Fennec.Database.Domain;
using Fennec.Processing;
using Fennec.Processing.Graph;

namespace Fennec.Tests.Filtering;

public class FilterConditionTests
{
    private readonly TraceEdge _trace = new (
            new TraceNodeKey(IPAddress.Parse("192.168.1.100")),
            new TraceNodeKey(IPAddress.Parse("10.0.0.1")),
            12345, 80,
            DataProtocol.Tcp, 1000, 10);

    [Fact]
    public void MatchWithExactAddressesPortsAndProtocol()
    {
        // Arrange
        var filter = new FilterCondition(
            new byte[] { 192, 168, 1, 100 },
            new byte[] { 255, 255, 255, 255 },
            12345,
            new byte[] { 10, 0, 0, 1 },
            new byte[] { 255, 255, 255, 255 },
            80,
            DataProtocol.Tcp,
            true
        );

        // Act
        var result = filter.MatchesTraceEdge(_trace);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MismatchOnExactSourceAddress()
    {
        // Arrange
        var filter = new FilterCondition(
            new byte[] { 192, 168, 1, 101 }, // Intentionally incorrect
            new byte[] { 255, 255, 255, 255 },
            12345,
            new byte[] { 10, 0, 0, 1 },
            new byte[] { 255, 255, 255, 255 },
            80,
            DataProtocol.Tcp,
            true
        );

        // Act
        var result = filter.MatchesTraceEdge(_trace);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchWithVagueAddressesExactPortAndProtocol()
    {
        // Arrange
        var filter = new FilterCondition(
            new byte[] { 192, 168, 1, 0 }, // Matched by mask
            new byte[] { 255, 255, 255, 0 },
            12345,
            new byte[] { 10, 0, 0, 0 }, // Matched by mask
            new byte[] { 255, 255, 255, 0 },
            80,
            DataProtocol.Tcp,
            true
        );

        // Act
        var result = filter.MatchesTraceEdge(_trace);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchWithWildCardAddressExactPortAndProtocol()
    {
        // Arrange
        var filter = new FilterCondition(
            new byte[] { 0, 0, 0, 0 }, // Ignored due to mask
            new byte[] { 0, 0, 0, 0 },
            12345,
            new byte[] { 0, 0, 0, 0 }, // Ignored due to mask
            new byte[] { 0, 0, 0, 0 },
            80,
            DataProtocol.Tcp,
            true
        );

        // Act
        var result = filter.MatchesTraceEdge(_trace);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MismatchWithProtocolAndExactAddressAndPort()
    {
        // Arrange
        var filter = new FilterCondition(
            new byte[] { 192, 168, 1, 100 },
            new byte[] { 255, 255, 255, 255 },
            12345,
            new byte[] { 10, 0, 0, 1 },
            new byte[] { 255, 255, 255, 255 },
            80,
            DataProtocol.Udp, // Intentional mismatch
            true
        );

        // Act
        var result = filter.MatchesTraceEdge(_trace);

        // Assert
        Assert.False(result);
    }
}