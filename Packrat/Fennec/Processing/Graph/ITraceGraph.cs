using System.Net;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Utils;

namespace Fennec.Processing.Graph;

// public record EdgeKey(IPAddress Source, IPAddress Target, DataProtocol Protocol);

public interface ITraceGraph
{
    public int EdgeCount { get; }
    public int NodeCount { get; }

    public SortedList<(IPAddress, IPAddress, ushort, ushort, DataProtocol), TraceEdge> Edges { get; }
    public SortedList<IPAddress, TraceNode> Nodes { get; }

    public void FillFromTraces(List<AggregateTrace> traces);
    
    public void FilterEdges(Func<TraceEdge, bool> filter);
    
    public void FilterNodes(Func<TraceNode, bool> filter);

    public void GroupEdges<T>(Func<(IPAddress, IPAddress, ushort, ushort, DataProtocol), TraceEdge, T> keySelector,
        Func<T, IEnumerable<TraceEdge>, TraceEdge> valueSelector);

    public void GroupNodes<T>(Func<IPAddress, TraceNode, T?> keySelector,
        Func<T, IEnumerable<TraceNode>, TraceNode> valueSelector) where T : notnull;

    public void AddEdge(TraceEdge traceEdge);
    public void AddNode(TraceNode traceNode);

    public void AddManyNodes(IEnumerable<TraceNode> nodes);
    public void AddManyEdges(IEnumerable<TraceEdge> edges);

    public void RemoveEdge((IPAddress, IPAddress, ushort, ushort, DataProtocol) key);
    public void RemoveNode(IPAddress key);

    public bool HasEdge((IPAddress, IPAddress, ushort, ushort, DataProtocol) key);
    public bool HasNode(IPAddress key);
}

public class TraceGraph : ITraceGraph
{
    public int EdgeCount => Edges.Count;
    public int NodeCount => Nodes.Count;

    // TODO: create an EdgeKey type
    public SortedList<(IPAddress, IPAddress, ushort, ushort, DataProtocol), TraceEdge> Edges { get; private set; } =
        new(new IpAddressPairComparer());

    public SortedList<IPAddress, TraceNode> Nodes { get; } = new(new IpAddressComparer());

    public void FillFromTraces(List<AggregateTrace> traces)
    {
        var nodes = traces
            .SelectMany<AggregateTrace, byte[]>(trace => new[] { trace.SourceIpBytes, trace.DestinationIpBytes })
            .GroupBy(t => t, new ByteArrayComparer())
            .Select(t => t.First())
            .Select(bytes => new TraceNode(new IPAddress(bytes), new IPAddress(bytes).ToString()));

        foreach (var node in nodes)
            AddNode(node);

        var edges = traces
            .Select(trace =>
                new TraceEdge(
                    new IPAddress(trace.SourceIpBytes),
                    new IPAddress(trace.DestinationIpBytes),
                    trace.SourcePort,
                    trace.DestinationPort,
                    trace.DataProtocol,
                    trace.PacketCount,
                    trace.ByteCount));

        foreach (var edge in edges)
            AddEdge(edge);
    }

    public void FilterEdges(Func<TraceEdge, bool> filter)
    {
        var potentialOrphans = new HashSet<IPAddress>();
        var removedEdges = new List<(IPAddress, IPAddress, ushort, ushort, DataProtocol)>();
        foreach (var edge in Edges)
        {
            if (filter(edge.Value))
                continue;

            removedEdges.Add(edge.Key);
            potentialOrphans.Add(edge.Value.Source);
            potentialOrphans.Add(edge.Value.Target);
        }

        foreach (var edge in removedEdges)
            Edges.Remove(edge);

        foreach (var potentialOrphan in potentialOrphans)
            RemoveNodeIfOrphan(potentialOrphan);
    }
    
    public void FilterNodes(Func<TraceNode, bool> filter)
    {
        var removedNodes = new List<IPAddress>();
        foreach (var node in Nodes)
        {
            if (filter(node.Value))
                continue;

            removedNodes.Add(node.Key);
        }

        foreach (var node in removedNodes)
            Nodes.Remove(node);

        var removedEdges = new List<(IPAddress, IPAddress, ushort, ushort, DataProtocol)>();
        foreach (var edge in Edges)
        {
            if (removedNodes.Contains(edge.Value.Source) || removedNodes.Contains(edge.Value.Target))
                removedEdges.Add(edge.Key);
        }
        
        // TODO: potentially the graph is at an invalid state here?
        foreach (var edge in removedEdges)
            Edges.Remove(edge);
    }

    public void AddEdge(TraceEdge traceEdge)
    {
        Edges.Add(
            (traceEdge.Source, traceEdge.Target, traceEdge.SourcePort, traceEdge.TargetPort, traceEdge.DataProtocol),
            traceEdge);
    }

    public void AddNode(TraceNode traceNode)
    {
        Nodes.Add(traceNode.Address, traceNode);
    }

    public void AddManyNodes(IEnumerable<TraceNode> nodes)
    {
        foreach (var node in nodes)
            Nodes.Add(node.Address, node);
    }

    public void AddManyEdges(IEnumerable<TraceEdge> edges)
    {
        foreach (var edge in edges)
            Edges.Add((edge.Source, edge.Target, edge.SourcePort, edge.TargetPort, edge.DataProtocol), edge);
    }

    public void RemoveEdge((IPAddress, IPAddress, ushort, ushort, DataProtocol) key)
    {
        Edges.Remove(key);
        RemoveNodeIfOrphan(key.Item1);
        RemoveNodeIfOrphan(key.Item2);
    }

    public void RemoveNode(IPAddress key)
    {
        Nodes.Remove(key);
    }

    public bool HasEdge((IPAddress, IPAddress, ushort, ushort, DataProtocol) key)
    {
        return Edges.ContainsKey(key);
    }

    public bool HasNode(IPAddress key)
    {
        return Nodes.ContainsKey(key);
    }

    public void GroupEdges<T>(Func<(IPAddress, IPAddress, ushort, ushort, DataProtocol), TraceEdge, T> keySelector,
        Func<T, IEnumerable<TraceEdge>, TraceEdge> valueSelector)
    {
        // TODO: do not check all nodes whether they are orphaned
        Edges = new SortedList<(IPAddress, IPAddress, ushort, ushort, DataProtocol), TraceEdge>(
            Edges.GroupBy(kp => keySelector(kp.Key, kp.Value), (key, value) =>
                valueSelector(key, value.Select(i => i.Value))
            ).ToDictionary(k => (k.Source, k.Target, k.SourcePort, k.TargetPort, k.DataProtocol), k => k),
            Edges.Comparer);

        foreach (var node in Nodes)
            RemoveNodeIfOrphan(node.Key);
    }

    public void GroupNodes<T>(Func<IPAddress, TraceNode, T?> keySelector,
        Func<T, IEnumerable<TraceNode>, TraceNode> valueSelector) where T : notnull
    {
        var dict = new Dictionary<T, List<TraceNode>>();
        foreach (var (nodeKey, nodeValue) in Nodes)
        {
            var key = keySelector(nodeKey, nodeValue);
            if (key == null)
                continue;
            
            if (!dict.ContainsKey(key))
                dict[key] = new List<TraceNode>();
            dict[key].Add(nodeValue);
        }

        foreach (var (key, value) in dict)
        {
            // if (value.Count == 1)
                // continue;

            // Create a new node and remove the old ones
            // Point all edges to the new node
            var newNode = valueSelector(key, value);
            // RemoveNode(key);
            AddNode(newNode);
            foreach (var node in value)
            {
                // Delete all old nodes
                Nodes.Remove(node.Address);
                RepointEdges(node.Address, newNode.Address);
            }
        }
    }
    
    private void RepointEdges(IPAddress oldAddress, IPAddress newAddress)
    {
        var edgesToRepoint = Edges.Where(kp =>
            kp.Value.Source.Equals(oldAddress) || kp.Value.Target.Equals(oldAddress)).ToList();
        foreach (var (oldEdgeKey, oldEdgeValue) in edgesToRepoint)
        {
            Edges.Remove(oldEdgeKey);

            // Deliberately set the ports to 0 to prevent inconsistent behavior
            // This is the case when different edges have different ports and are later filtered
            var newKey = (
                oldEdgeKey.Item1.Equals(oldAddress) ? newAddress : oldEdgeKey.Item1,
                oldEdgeKey.Item2.Equals(oldAddress) ? newAddress : oldEdgeKey.Item2,
                (ushort) 0, (ushort) 0, oldEdgeKey.Item5);

            if (Edges.TryGetValue(newKey, out var existingEdge))
            {
                existingEdge.PacketCount += oldEdgeValue.PacketCount;
                existingEdge.ByteCount += oldEdgeValue.ByteCount;
                continue;
            }
            
            Edges.Add(newKey, oldEdgeValue);
        }
    }

    private void RemoveNodeIfOrphan(IPAddress key)
    {
        var contained = Edges.Keys.Any(kp => kp.Item1.Equals(key) || kp.Item2.Equals(key));
        if (contained)
            return;

        Nodes.Remove(key);
    }
}