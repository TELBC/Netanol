using System.Net;
using System.Net.Sockets;
using Fennec.Parsers;
using Fennec.Services;
using NSubstitute;
using Serilog;

namespace Fennec.Tests.Parsers;

public class NetFlow5ParserTests
{
    [Fact]
    public void ParseNetFlow9Message()
    {
        // Arrange
        var substituteLogger = Substitute.For<ILogger>();

        var parser = new NetFlow5Parser(substituteLogger);
        var netflowData = Convert.FromBase64String(
            "AAUAAQG+2TFl4ITBAAAAAAAAAAAAAAAACgAAAgoAAAMAAAAAAAMABQAAAAAAAAAAAb3u0QG+2TEQkgBQAAAGAQACAAMgHwAA");

        var dataUdpReceiveResult = new UdpReceiveResult(netflowData, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));

        // Act
        var data = parser.Parse(dataUdpReceiveResult);

        // Assert
        Assert.NotNull(data);
        Assert.Single(data);
    }
}