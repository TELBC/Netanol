using System.Net;
using System.Net.Sockets;
using Fennec.Parsers;
using Fennec.Services;
using NSubstitute;
using Serilog;

namespace Fennec.Tests.Parsers;

public class NetFlow9ParserTests
{
    [Fact]
    public void ParseNetFlow9Message()
    {
        // Arrange
        var substituteLogger = Substitute.For<ILogger>();
        var substituteMetricService = Substitute.For<IMetricService>();

        var parser = new NetFlow9Parser(substituteLogger, substituteMetricService);
        var netflowTemplates = Convert.FromBase64String(
            "AAkAAgBaBI9k0SU7AAAAaAAAAAIBAwBRAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAzC8AxwKgAlgG71i0GAAAAAQBZiYAAWYmAAAApaQAAAAAAAABAAAAAAAAAAAEAAAA8AQMADQAbABAAHAAQAAgABAAMAAQABwACAAsAAgAEAAEAMAAEABYABAAVAAQACgAEAAEACAACAAg=");
        var netflowData = Convert.FromBase64String(
            "AAkAAwBZtm9k0SUnAAAAZwAAAAIBAwDrAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACin4bqwKgAlgG70kIGAAAAAQBY7TMAWTtgAAApaQAAAAAAAAaoAAAAAAAAAAMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJJCmzbAqACWaYz32AYAAAABAFk7YABZO2AAAClpAAAAAAAAASYAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEkIae8CoAJYBu9ZgBgAAAAEAWTtgAFk7YAAAKWkAAAAAAAAAjAAAAAAAAAAC");

        var templatesUdpReceiveResult =
            new UdpReceiveResult(netflowTemplates, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
        var dataUdpReceiveResult = new UdpReceiveResult(netflowData, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));

        // Act
        var templates = parser.Parse(templatesUdpReceiveResult);
        var data = parser.Parse(dataUdpReceiveResult).ToList();

        // Assert
        Assert.NotNull(templates);
        Assert.NotNull(data);

        Assert.Empty(templates);
        Assert.Single(data);
        Assert.Equal((ulong)3, data.First().PacketCount);
    }
}