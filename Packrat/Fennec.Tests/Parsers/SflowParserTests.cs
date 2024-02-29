using System.Net;
using System.Net.Sockets;
using Fennec.Parsers;
using Fennec.Services;
using NSubstitute;
using Serilog;

namespace Fennec.Tests.Parsers;

public class SflowParserTests
{
    [Fact]
    public void ParseSflowFlowSample()
    {
        // Arrange
        var substituteLogger = Substitute.For<ILogger>();
        
        var parser = new SflowParser(substituteLogger);
        var flowSample = Convert.FromBase64String("AAAABQAAAAGsFSMRAAAAAQAAAaJnPn8IAAAAAQAAAAEAAACIAAAABgAABBMAAAgAAAAwAAAAAAAAAAQYAAAEEwAAAAEAAAABAAAAYAAAAAEAAABSAAAABAAAAE4AHCOfFQsAGbndsmSBAAAgCABFAAA8XAcAAHwBSKCsFSD+rBUg8QgAl2GpSAyyYWJjZGVmZ2hpamtsbW5vcHFyc3R1dndhYmNkZWZnaGkAAA==");
        
        var flowSampleUdpReceiveResult = new UdpReceiveResult(flowSample, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
        
        // Act
        var resultSamples = parser.Parse(flowSampleUdpReceiveResult).ToList();
        
        // Assert
        Assert.NotNull(resultSamples);
        Assert.Single(resultSamples);
        Assert.Equal(resultSamples.First().DstIp, IPAddress.Parse("172.21.32.241"));
    }

    [Fact]
    public void ParseSflowCounterSample()
    {
        // Arrange
        var substituteLogger = Substitute.For<ILogger>();
        
        var parser = new SflowParser(substituteLogger);
        var flowSample = Convert.FromBase64String("AAAABQAAAAGsFSMRAAAAAQAAAZ9nPdcQAAAAAQAAAAIAAABsAAAhJQAABAwAAAABAAAAAQAAAFgAAAQMAAAABgAAAAAF9eEAAAAAAQAAAAMAAAAAAYwszAAAm4MAApAWAAH2cwAAAAAAAAAAAAAAAAAAAAAAUz3BAACgtwAAIYcAAAjXAAAAAAAAAAAAAAAA");
        
        var flowSampleUdpReceiveResult = new UdpReceiveResult(flowSample, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
        
        // Act
        var resultSamples = parser.Parse(flowSampleUdpReceiveResult);
        
        // Assert
        Assert.NotNull(resultSamples);
        Assert.Empty(resultSamples); // CounterSample is not relevant for topology visualization
    }
}