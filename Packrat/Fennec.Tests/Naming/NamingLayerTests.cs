using System.Net;
using Fennec.Database;
using Fennec.Processing;
using Fennec.Processing.Graph;

namespace Fennec.Tests.Naming;

public class NamingLayerTests
{
    private static readonly IPAddress Ip1 = IPAddress.Parse("1.1.1.1");
    private static readonly IPAddress Ip2 = IPAddress.Parse("2.2.2.2");


    private readonly List<AggregateTrace> _aggregateTraces = new()
    {
        new AggregateTrace
        {
            SourceIpBytes = Ip1.GetAddressBytes(),
            SourceDnsName = "host1.local",
            DestinationIpBytes = Ip2.GetAddressBytes(),
            DestinationDnsName = "host2.local",
        }
    };

    [Fact]
    public void CorrectlyAssignsName()
    {
        // Arrange
        var graph = new TraceGraph();
        graph.FillFromTraces(_aggregateTraces);

        var layer = new NamingLayer
        {
            Matchers = new List<NamingAssigner>
            {
                new(new byte[] { 1, 1, 1, 1 }, new byte[] { 255, 255, 255, 255 }, true, "Main DNS Server")
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.Contains(graph.Nodes, n => n.Value.Name.Equals("Main DNS Server"));
    }

    [Fact]
    public void DoNotAssignNameToExcluded()
    {
        // Arrange
        var graph = new TraceGraph();
        graph.FillFromTraces(_aggregateTraces);

        var layer = new NamingLayer
        {
            Matchers = new List<NamingAssigner>
            {
                new(new byte[] { 1, 1, 1, 0 }, new byte[] { 255, 255, 255, 0 }, false, ""),
                new(new byte[] { 1, 1, 1, 1 }, new byte[] { 255, 255, 255, 255 }, true, "Main DNS Server")
            }
        };

        // Act
        layer.Execute(graph, null!);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.Contains(graph.Nodes, n => n.Value.Name.Equals("1.1.1.1"));
    }

    [Fact]
    public void OverwritesWithDnsName_And_SetsDefinedName()
    {
        // Arrange
        var graph = new TraceGraph();
        graph.FillFromTraces(_aggregateTraces);

        var layer = new NamingLayer
        {
            Matchers = new List<NamingAssigner>
            {
                // new(new byte[]{ 1, 0, 0, 0}, new byte[]{255, 0, 0, 0}, true, ""),
                new (new byte[]{ 2, 2,2,2}, new byte[] {255,255,255,255}, true, "Server2")
            },
            OverwriteWithDns = true
        };
        
        // Act  
        layer.Execute(graph, null!);
        
        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.Contains(graph.Nodes, n => n.Value.Name.Equals("host1.local"));
        Assert.Contains(graph.Nodes, n => n.Value.Name.Equals("Server2"));
    }
    
}