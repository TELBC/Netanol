using System.Net;
using Fennec.Database.Domain;
using Fennec.Processing.Graph;

namespace Fennec.Tests;

public class TraceGraphTests
{
    private TraceNode Node(string id)
    {
        return new TraceNode(IPAddress.Parse(id), id);
    }

    private TraceEdge Edge(string from, string target)
    {
        return new TraceEdge(IPAddress.Parse(from), IPAddress.Parse(target), 0, 0, DataProtocol.Tcp, 0, 0);
    }

    [Fact]
    public void AddNodeAndRemoveNode_WorkCorrectly()
    {
        // Arrange
        var graph = new TraceGraph();
        var address = IPAddress.Parse("10.10.10.10");
        var node = new TraceNode(address, "");


        // Act
        graph.AddNode(node);
        Assert.Equal(1, graph.NodeCount);
        Assert.True(graph.HasNode(address));
        graph.RemoveNode(address);

        // Assert
        Assert.Equal(0, graph.NodeCount);
        Assert.False(graph.HasNode(address));
    }

    [Fact]
    public void RemoveEdges_RemovesOrphanNodes()
    {
        // Arrange
        var n1 = Node("1.1.1.1");
        var n2 = Node("2.2.2.2");
        var n3 = Node("3.3.3.3");
        var e1 = Edge("1.1.1.1", "2.2.2.2");
        var e2 = Edge("2.2.2.2", "3.3.3.3");
        var g = new TraceGraph();

        Assert.False(n1.Equals(n2));

        // Act
        g.AddManyNodes(new[] { n1, n2, n3 });
        g.AddManyEdges(new[] { e1, e2 });
        g.FilterEdges(e => e.Source.Equals(IPAddress.Parse("2.2.2.2")));
        
        // Assert
        Assert.Equal(1, g.EdgeCount);
        Assert.Equal(IPAddress.Parse("3.3.3.3"), g.Edges.First().Value.Target);
    }
}