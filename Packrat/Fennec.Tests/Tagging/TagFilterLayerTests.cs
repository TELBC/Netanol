using System.Net;
using Fennec.Database.Domain;
using Fennec.Processing;
using Fennec.Processing.Graph;

namespace Fennec.Tests.Tagging;

public class TagFilterLayerTests
{
    private static TraceGraph CreateGraph()
    {
        var n1 = new TraceNode(IPAddress.Parse("1.1.1.1"), "") { Tags = new List<string> { "infra-1", "switch" } };
        var n2 = new TraceNode(IPAddress.Parse("2.2.2.2"), "") { Tags = new List<string> { "infra-2" } };
        var n3 = new TraceNode(IPAddress.Parse("3.3.3.3"), "") { Tags = new List<string> { "infra-3", "switch" } };
        var n4 = new TraceNode(IPAddress.Parse("4.4.4.4"), "") { Tags = new List<string>() };

        var e1 = new TraceEdge(n1.Address, n2.Address, 0, 0, DataProtocol.Unknown, 0, 0);
        var e2 = new TraceEdge(n2.Address, n3.Address, 0, 0, DataProtocol.Unknown, 0, 0);
        var e3 = new TraceEdge(n3.Address, n4.Address, 0, 0, DataProtocol.Unknown, 0, 0);

        var graph = new TraceGraph();
        graph.AddManyNodes(new[] { n1, n2, n3, n4 });
        graph.AddManyEdges(new[] { e1, e2, e3 });
        return graph;
    }

    [Fact]
    public void RemovesMatchingNodes_MatchHasNone()
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new TagFilterLayer
        {
            ImplicitInclude = true,
            Conditions = new List<TagFilterCondition>
            {
                new()
                {
                    Type = TagFilterConditionType.MatchesNone, Regexes = new List<string> { "switch" }, Include = false
                }
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal(2, graph.NodeCount);
        Assert.DoesNotContain(graph.Nodes.Values, n => n.Address.Equals(IPAddress.Parse("2.2.2.2")));
        Assert.DoesNotContain(graph.Edges.Values, e => e.Source.Equals(IPAddress.Parse("1.1.1.1")));
    }

    [Fact]
    public void KeepsMatchingNodes_MatchHasAny()
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new TagFilterLayer
        {
            ImplicitInclude = false,
            Conditions = new List<TagFilterCondition>
            {
                new() { Type = TagFilterConditionType.MatchesAny, Regexes = new List<string> { "infra-" }, Include = true }
            }
        };
        
        // Act
        layer.Execute(graph, null!);
        
        // Assert
        Assert.Equal(3, graph.NodeCount);
        Assert.DoesNotContain(graph.Nodes.Values, n => n.Address.Equals(IPAddress.Parse("4.4.4.4")));
        Assert.Equal(2, graph.EdgeCount);
    }

    [Fact]
    public void KeepsMatchingNodes_MatchHasAll()
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new TagFilterLayer
        {
            ImplicitInclude = false,
            Conditions = new List<TagFilterCondition>
            {
                new() { Type = TagFilterConditionType.MatchesAll, Regexes = new List<string> { "infra-", "switch" }, Include = true }
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal(2, graph.NodeCount);
        Assert.Contains(graph.Nodes.Values, n => n.Address.Equals(IPAddress.Parse("3.3.3.3")));
    }

    [Fact]
    public void KeepsMatchingNodes_MatchHasExactly()
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new TagFilterLayer
        {
            ImplicitInclude = false,
            Conditions = new List<TagFilterCondition>
            {
                new() { Type = TagFilterConditionType.MatchesExactly, Regexes = new List<string> { "infra-1", "switch" }, Include = true }
            }
        };
        
        // Act
        layer.Execute(graph, null!);
        
        // Assert
        Assert.Single(graph.Nodes.Values);
        Assert.Empty(graph.Edges.Values);
    }

    [Fact]
    public void RemovesSomeMatchingNodes_ComplexQuery()
    {
        // Arrange
        var graph = CreateGraph();
        var layer = new TagFilterLayer
        {
            ImplicitInclude = false,
            Conditions = new List<TagFilterCondition>
            {
                new() { Type = TagFilterConditionType.MatchesAny, Regexes = new List<string> { "infra-1" }, Include = false },
                new() { Type = TagFilterConditionType.MatchesAll, Regexes = new List<string> { "infra-", "switch" }, Include = true }
            }
        };
        
        // Act
        layer.Execute(graph, null!);
        
        // Assert
        Assert.Single(graph.Nodes.Values);
        Assert.Empty(graph.Edges.Values);
        Assert.Equal("3.3.3.3", graph.Nodes.Values.Single().Address.ToString());
    }
}