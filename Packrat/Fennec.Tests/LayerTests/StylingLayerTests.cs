using System.Net;
using Fennec.Database.Domain;
using Fennec.Processing;
using Fennec.Processing.Graph;

namespace Fennec.Tests.LayerTests;

public class StylingLayerTests
{
    private ITraceGraph CreateGraph()
    {
        var ip1 = IPAddress.Parse("1.1.1.1");
        var ip2 = IPAddress.Parse("1.1.1.2");
        var n1 = new TraceNode(ip1, ip1.ToString());
        var n2 = new TraceNode(ip2, ip2.ToString());

        var e1 = new TraceEdge(n1.Key, n2.Key, 0, 0,
            DataProtocol.Tcp, 1000, 100);

        var graph = new TraceGraph();
        graph.AddManyNodes(new[] { n1, n2 });
        graph.AddManyEdges(new[] { e1 });
        return graph;
    }

    [Fact]
    public void NodeStyler_DoesNotStyle()
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new StylingLayer
        {
            NodeStyler = new NodeStyler
            {
                SetColor = true
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Null(graph.Nodes.Values.First().HexColor);
    }

    [Fact]
    public void NodeStyler_Styles()
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new StylingLayer
        {
            NodeStyler = new NodeStyler
            {
                SetColor = true,
                Assignments = new List<NodeColorAssignment>
                {
                    new(
                        new IpAddressMatcher(new byte[] { 1, 1, 1, 1 }, new byte[] { 255, 255, 255, 255 }, true),
                        "#223344")
                }
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal("#223344", graph.Nodes.Values.First().HexColor);
    }

    [Fact]
    public void NodeStyler_IgnoresNull()
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new StylingLayer
        {
            NodeStyler = new NodeStyler
            {
                SetColor = true,
                Assignments = new List<NodeColorAssignment>
                {
                    new(
                        new IpAddressMatcher(
                            new byte[] { 1, 1, 1, 1 },
                            new byte[] { 255, 255, 255, 255 }, true),
                        null)
                }
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Null(graph.Nodes.Values.First().HexColor);
    }

    // Different 
    [Theory]
    [InlineData(ScoringMode.ByteCount, 10f, 0, 10)]
    [InlineData(ScoringMode.PacketCount, 12f, 2, 12)]
    public void EdgeStyler_SetsWidthBasedOn(ScoringMode scoringMode, float expected, ulong min, ulong max)
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new StylingLayer
        {
            EdgeStyler = new EdgeStyler
            {
                SetWidth = true,
                WidthScoringMode = scoringMode,
                EdgeMinWidth = min,
                EdgeMaxWidth = max
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal(expected, graph.Edges.Values.First().Width);
    }

    [Theory]
    [InlineData(ScoringMode.ByteCount, 0f, 10f, 5f)]
    [InlineData(ScoringMode.PacketCount, 0f, 5f, 10f)]
    [InlineData(ScoringMode.Calculated, 0f, 10f, 7.14285755f)]
    public void EdgeStyler_ComplexSetsWidth(ScoringMode scoringMode, float expected1, float expected2, float expected3)
    {
        // Arrange
        var graph = new TraceGraph();
        var ip1 = IPAddress.Parse("1.1.1.1");
        var ip2 = IPAddress.Parse("2.2.2.2");
        var ip3 = IPAddress.Parse("3.3.3.3");

        var n1 = new TraceNode(ip1, ip1.ToString());
        var n2 = new TraceNode(ip2, ip2.ToString());
        var n3 = new TraceNode(ip3, ip3.ToString());

        var e1 = new TraceEdge(n1.Key, n2.Key, 0, 0,
            DataProtocol.Tcp, 500, 1_000);
        var e2 = new TraceEdge(n2.Key, n3.Key, 0, 0,
            DataProtocol.Tcp, 750, 10_000);
        var e3 = new TraceEdge(n3.Key, n1.Key, 0, 0,
            DataProtocol.Tcp, 1_000, 5_500);

        graph.AddManyNodes(new[] { n1, n2 });
        graph.AddManyEdges(new[] { e1, e2, e3 });

        var layer = new StylingLayer
        {
            EdgeStyler = new EdgeStyler
            {
                SetWidth = true,
                WidthScoringMode = scoringMode,
                EdgeMinWidth = 0,
                EdgeMaxWidth = 10
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal(expected1, e1.Width);
        Assert.Equal(expected2, e2.Width);
        Assert.Equal(expected3, e3.Width);
    }

    [Theory]
    [InlineData(false, "#000000", "#000000", "#000000")]
    [InlineData(true, "#FFFFFF", "#000000", "#7F7F7F")]
    public void EdgeStyler_UsesDefaultColorCorrectly(bool interpolate, string expect1, string expect2, string expect3)
    {
        // Arrange
        var graph = new TraceGraph();

        var ip1 = IPAddress.Parse("1.1.1.1");
        var ip2 = IPAddress.Parse("2.2.2.2");
        var ip3 = IPAddress.Parse("3.3.3.3");

        var n1 = new TraceNode(ip1, ip1.ToString());
        var n2 = new TraceNode(ip2, ip2.ToString());
        var n3 = new TraceNode(ip3, ip3.ToString());

        var e1 = new TraceEdge(n1.Key, n2.Key, 0, 0,
            DataProtocol.Tcp, 500, 1_000);
        var e2 = new TraceEdge(n2.Key, n3.Key, 0, 0,
            DataProtocol.Tcp, 750, 10_000);
        var e3 = new TraceEdge(n3.Key, n1.Key, 0, 0,
            DataProtocol.Tcp, 1_000, 5_500);

        graph.AddManyNodes(new[] { n1, n2 });
        graph.AddManyEdges(new[] { e1, e2, e3 });

        var layer = new StylingLayer
        {
            EdgeStyler = new EdgeStyler
            {
                SetWidth = false,
                SetColor = true,
                ColorScoringMode = ScoringMode.ByteCount,
                InterpolateColors = interpolate,
                UseProtocolColors = false
            }
        };


        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal(expect1, e1.HexColor);
        Assert.Equal(expect2, e2.HexColor);
        Assert.Equal(expect3, e3.HexColor);
    }

    [Fact]
    public void EdgeStyler_UsesProtocolColorsCorrectly_And_InterpolatesWithSingleEdge()
    {
        // Arrange
        var graph = new TraceGraph();

        var ip1 = IPAddress.Parse("1.1.1.1");
        var ip2 = IPAddress.Parse("2.2.2.2");

        var n1 = new TraceNode(ip1, ip1.ToString());
        var n2 = new TraceNode(ip2, ip2.ToString());

        var e1 = new TraceEdge(n1.Key, n1.Key, 0, 0,
            DataProtocol.Tcp, 500, 1_000);

        graph.AddManyNodes(new[] { n1, n2 });
        graph.AddManyEdges(new[] { e1 });

        var layer = new StylingLayer
        {
            EdgeStyler = new EdgeStyler
            {
                SetWidth = false,
                SetColor = true,
                ColorScoringMode = ScoringMode.ByteCount,
                InterpolateColors = true,
                UseProtocolColors = true,
                ProtocolColors = new Dictionary<DataProtocol, ColorRange>
                {
                    { DataProtocol.Tcp, new ColorRange("#223344", "#556677") }
                }
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal("#556677", e1.HexColor);
    }

    [Fact]
    public void EdgeStyler_UsesDefinedDefaultColorCorrectly()
    {
        // Arrange
        var graph = new TraceGraph();

        var ip1 = IPAddress.Parse("1.1.1.1");
        var ip2 = IPAddress.Parse("2.2.2.2");
        var n1 = new TraceNode(ip1, ip1.ToString());
        var n2 = new TraceNode(ip2, ip2.ToString());

        var e1 = new TraceEdge(n1.Key, n2.Key, 0, 0,
            DataProtocol.Tcp, 500, 1_000);
        var e2 = new TraceEdge(n2.Key, n1.Key, 0, 0,
            DataProtocol.Tcp, 750, 10_000);

        graph.AddManyNodes(new[] { n1, n2 });
        graph.AddManyEdges(new[] { e1, e2 });

        var layer = new StylingLayer
        {
            EdgeStyler = new EdgeStyler
            {
                SetWidth = false,
                SetColor = true,
                InterpolateColors = false,
                UseProtocolColors = false,
                ProtocolColors = new Dictionary<DataProtocol, ColorRange>
                {
                    { DataProtocol.Unknown, new ColorRange("#000000", "#FFFFFF") }
                }
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal("#FFFFFF", e1.HexColor);
        Assert.Equal("#FFFFFF", e2.HexColor);
    }
}