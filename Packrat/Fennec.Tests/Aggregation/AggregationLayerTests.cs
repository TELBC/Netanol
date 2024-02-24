using System.Net;
using Fennec.Database.Domain;
using Fennec.Processing;
using Fennec.Processing.Graph;

namespace Fennec.Tests.Aggregation;

public class AggregationLayerTests
{
    [Fact]
    public void Aggregates_TwoNodesIntoOne()
    {
        // Arrange
        var n1 = new TraceNode(IPAddress.Parse("1.1.1.1"), "");
        var n2 = new TraceNode(IPAddress.Parse("1.1.1.2"), "");
        var n3 = new TraceNode(IPAddress.Parse("2.2.2.1"), "");
        var n4 = new TraceNode(IPAddress.Parse("2.2.2.2"), "");

        var e1 = new TraceEdge(n1.Address, n3.Address, 10, 20, DataProtocol.Unknown, 10, 1000);
        var e2 = new TraceEdge(n2.Address, n4.Address, 10, 20, DataProtocol.Unknown, 10, 1000);

        var graph = new TraceGraph();
        var layer = new AggregationLayer
        {
            Matchers = new List<IpAddressMatcher>
            {
                new(new byte[] { 1, 1, 1, 0 }, new byte[] { 255, 255, 255, 0 }, true)
            }
        };

        // Act
        graph.AddManyNodes(new[] { n1, n2, n3, n4 });
        graph.AddManyEdges(new[] { e1, e2 });
        layer.Execute(graph);

        // Assert
        Assert.Equal(3, graph.NodeCount);
        Assert.Equal(2, graph.EdgeCount);
    }

    [Fact]
    public void Aggregates_ThreeNodesIntoTwo_MergeTwoEdges()
    {
        // Arrange
        var n1 = new TraceNode(IPAddress.Parse("1.1.1.1"), "");
        var n2 = new TraceNode(IPAddress.Parse("1.1.1.2"), "");
        var n3 = new TraceNode(IPAddress.Parse("1.1.2.1"), "");
        var n4 = new TraceNode(IPAddress.Parse("1.1.2.2"), "");
        var n5 = new TraceNode(IPAddress.Parse("2.2.2.1"), "");
        var n6 = new TraceNode(IPAddress.Parse("2.2.2.2"), "");
        
        var e1 = new TraceEdge(n1.Address, n5.Address, 10, 20, DataProtocol.Unknown, 10, 1000);
        var e2 = new TraceEdge(n2.Address, n6.Address, 10, 20, DataProtocol.Unknown, 10, 1000);
        var e3 = new TraceEdge(n3.Address, n5.Address, 10, 20, DataProtocol.Unknown, 10, 1000);
        var e4 = new TraceEdge(n4.Address, n5.Address, 10, 20, DataProtocol.Unknown, 10, 1000);
        var e5 = new TraceEdge(n5.Address, n1.Address, 10, 20, DataProtocol.Unknown, 10, 1000);
        var e6 = new TraceEdge(n6.Address, n4.Address, 10, 20, DataProtocol.Unknown, 10, 1000);

        var graph = new TraceGraph();
        var layer = new AggregationLayer
        {
            Matchers = new List<IpAddressMatcher>
            {
                new(new byte[] { 1, 1, 2, 1 }, new byte[] { 255, 255, 255, 255 }, false),
                new(new byte[] { 1, 1, 2, 0 }, new byte[] { 255, 255, 255, 0 }, true),
                new(new byte[] { 1, 1, 0, 0 }, new byte[] { 255, 255, 0, 0 }, true)
            }
        };
        
        // Act
        graph.AddManyNodes(new[] { n1, n2, n3, n4, n5, n6 });
        graph.AddManyEdges(new[] { e1, e2, e3, e4, e5, e6 });
        layer.Execute(graph);
        
        // Assert
        Assert.Equal(5, graph.NodeCount);
        Assert.Equal(6, graph.EdgeCount);
    }

    [Fact]
    public void Aggregates_TwoNodesIntoOne_MergeTwoEdges()
    {
        // Arrange
        var n1 = new TraceNode(IPAddress.Parse("1.1.1.1"), "");
        var n2 = new TraceNode(IPAddress.Parse("1.1.1.2"), "");
        var n3 = new TraceNode(IPAddress.Parse("2.2.2.1"), "");

        var e1 = new TraceEdge(n1.Address, n3.Address, 10, 20, DataProtocol.Unknown, 10, 1000);
        var e2 = new TraceEdge(n2.Address, n3.Address, 10, 20, DataProtocol.Unknown, 10, 1000);

        var graph = new TraceGraph();
        var layer = new AggregationLayer
        {
            Matchers = new List<IpAddressMatcher>
            {
                new(new byte[] { 1, 1, 1, 0 }, new byte[] { 255, 255, 255, 0 }, true)
            }
        };

        // Act
        graph.AddManyNodes(new[] { n1, n2, n3 });
        graph.AddManyEdges(new[] { e1, e2 });
        layer.Execute(graph);

        // Assert
        Assert.Equal(2, graph.NodeCount);
        Assert.Equal(1, graph.EdgeCount);
        Assert.Contains(graph.Nodes,
            n => n.Value.Address.Equals(IPAddress.Parse("1.1.1.0")));
    }
}