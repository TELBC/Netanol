using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Processing;
using Fennec.Processing.Graph;

namespace Fennec.Tests.Filtering;

public class FilterListTests
{
    [Fact]
    public void FilterRemovesNonMatchingTraces()
    {
        // Arrange
        var conditions = new List<FilterCondition>
        {
            new(new byte[] { 192, 168, 1, 100 }, new byte[] { 255, 255, 255, 255 }, null, new byte[] { 10, 0, 0, 1 },
                new byte[] { 255, 255, 255, 255 }, null, null, true)
        };
        var filterList = new FilterList(false, conditions);
        var aggregateTraces = new List<AggregateTrace>
        {
            new(new byte[] { 192, 168, 1, 101 }, new byte[] { 10, 0, 0, 2 }, 12345, 80, DataProtocol.Tcp, 100, 1000)
        };
        var graph = new TraceGraph();
        graph.FillFromTraces(aggregateTraces);

        // Act
        filterList.Filter(graph);

        // Assert
        Assert.Empty(graph.Nodes);
        Assert.Empty(graph.Edges);
    }


    [Fact]
    public void FilterKeepsMatchingTraces()
    {
        // Arrange
        var conditions = new List<FilterCondition>
        {
            // Matching condition
            new(new byte[] { 192, 168, 1, 100 }, new byte[] { 255, 255, 255, 255 }, 12345, new byte[] { 10, 0, 0, 1 },
                new byte[] { 255, 255, 255, 255 }, 80, DataProtocol.Tcp, true)
        };
        var filterList = new FilterList(false, conditions);
        var aggregateTraces = new List<AggregateTrace>
        {
            new(new byte[] { 192, 168, 1, 100 }, new byte[] { 10, 0, 0, 1 }, 12345, 80, DataProtocol.Tcp, 100, 1000)
        };
        var graph = new TraceGraph();
        graph.FillFromTraces(aggregateTraces);

        // Act
        filterList.Filter(graph);

        // Assert
        Assert.Single(graph.Edges);
        Assert.Equal(2, graph.NodeCount);
    }


    [Fact]
    public void ImplicitIncludeKeepsUnmatchedTraces()
    {
        // Arrange
        var conditions = new List<FilterCondition>(); // No conditions, meaning nothing should match
        var filterList = new FilterList(true, conditions); // Implicit include is true
        var aggregateTraces = new List<AggregateTrace>
        {
            new(new byte[] { 192, 168, 1, 102 }, new byte[] { 10, 0, 0, 2 }, 54321, 8080, DataProtocol.Udp, 50, 500)
        };
        var graph = new TraceGraph();
        graph.FillFromTraces(aggregateTraces);

        // Act
        filterList.Filter(graph);

        // Assert
        Assert.Single(graph.Edges);
        Assert.Equal(2, graph.NodeCount);
    }

    [Fact]
    public void FilterRemovesTracesBasedOnIncludeFlag()
    {
        // Arrange
        var conditions = new List<FilterCondition>
        {
            // Condition that matches but specifies exclusion
            new(new byte[] { 192, 168, 1, 100 }, new byte[] { 255, 255, 255, 255 }, 12345, new byte[] { 10, 0, 0, 1 },
                new byte[] { 255, 255, 255, 255 }, 80, DataProtocol.Tcp, false)
        };
        var filterList = new FilterList(true, conditions);
        var aggregateTraces = new List<AggregateTrace>
        {
            new(new byte[] { 192, 168, 1, 100 }, new byte[] { 10, 0, 0, 1 }, 12345, 80, DataProtocol.Tcp, 100, 1000)
        };
        var graph = new TraceGraph();
        graph.FillFromTraces(aggregateTraces);

        // Act
        filterList.Filter(graph);

        // Assert
        Assert.Empty(graph.Edges);
        Assert.Empty(graph.Nodes);
    }

    [Fact]
    public void FilterWithMultipleConditions()
    {
        // Arrange
        var conditions = new List<FilterCondition>
        {
            new(new byte[] { 192, 168, 1, 100 }, new byte[] { 255, 255, 255, 255 }, 12345, new byte[] { 10, 0, 0, 1 },
                new byte[] { 255, 255, 255, 255 }, 80, DataProtocol.Tcp, true),
            new(new byte[] { 192, 168, 1, 101 }, new byte[] { 255, 255, 255, 255 }, 54321, new byte[] { 10, 0, 0, 2 },
                new byte[] { 255, 255, 255, 255 }, 8080, DataProtocol.Udp, true)
        };
        var filterList = new FilterList(false, conditions);
        var aggregateTraces = new List<AggregateTrace>
        {
            new(new byte[] { 192, 168, 1, 100 }, new byte[] { 10, 0, 0, 1 }, 12345, 80, DataProtocol.Tcp, 100, 1000),
            new(new byte[] { 192, 168, 1, 101 }, new byte[] { 10, 0, 0, 2 }, 54321, 8080, DataProtocol.Udp, 50, 500),
            new(new byte[] { 192, 168, 1, 100 }, new byte[] { 10, 0, 0, 2 }, 12345, 3123, DataProtocol.Tcp, 10, 10)
        };
        var graph = new TraceGraph();
        graph.FillFromTraces(aggregateTraces);

        // Act
        filterList.Filter(graph);

        // Assert
        Assert.Equal(2, graph.EdgeCount); // Both traces match different conditions and should be kept
    }
}