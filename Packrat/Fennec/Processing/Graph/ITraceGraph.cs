using System.Net;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Utils;

namespace Fennec.Processing.Graph;

/// <summary>
///     Defines the unique features that distinguish a <see cref="TraceEdge" />.
/// </summary>
/// <param name="Source">The <see cref="IPAddress" /> of the source.</param>
/// <param name="SourcePort">
///     The port on the source. `default` (i.e `0`) if <see cref="TraceEdge" /> is not aggregated by
///     port.
/// </param>
/// <param name="Target">The <see cref="IPAddress" /> of the destination.</param>
/// <param name="TargetPort">
///     The port of the target.`default` (i.e `0`) if <see cref="TraceEdge" /> is not aggregated by
///     port.
/// </param>
/// <param name="DataProtocol">Which <see cref="DataProtocol" /> was used during transit.</param>
public record TraceEdgeKey(
    TraceNodeKey Source,
    ushort SourcePort,
    TraceNodeKey Target,
    ushort TargetPort,
    DataProtocol DataProtocol);

/// <summary>
///     Defines the unique features that distinguish a <see cref="TraceNode" />.
/// </summary>
/// <param name="Address">The <see cref="IPAddress" /> of the node.</param>
public record TraceNodeKey(IPAddress Address);

public interface ITraceGraph
{
    /// <summary>
    ///     The total count of <see cref="TraceEdge" />s in this graph. Equal to `Edges.Count`.
    /// </summary>
    public int EdgeCount { get; }

    /// <summary>
    ///     The total count of <see cref="TraceNode" />s in this graph. Equal to `Nodes.Count`.
    /// </summary>
    public int NodeCount { get; }

    /// <summary>
    ///     A sorted list of <see cref="TraceEdge" />s in this graph where the key is a combinations of features that make
    ///     a <see cref="TraceEdge" /> unique and the value being the <see cref="TraceEdge" />.
    /// </summary>
    public SortedList<TraceEdgeKey, TraceEdge> Edges { get; }

    /// <summary>
    ///     A sortedlist of <see cref="TraceNode" />s in this graph where the key is a combination of features that make
    ///     a <see cref="TraceEdge" /> unique and the value being the <see cref="TraceEdge" />.
    /// </summary>
    public SortedList<TraceNodeKey, TraceNode> Nodes { get; }

    /// <summary>
    ///     Only retain <see cref="TraceEdge" /> for which the <paramref name="filter" /> function returns `true`.
    /// </summary>
    /// <param name="filter"></param>
    public void FilterEdges(Func<TraceEdge, bool> filter);

    /// <summary>
    ///     Only retain <see cref="TraceEdge" />s for which the <paramref name="filter" /> function returns `true`.
    /// </summary>
    public void FilterNodes(Func<TraceNode, bool> filter);

    /// <summary>
    ///     Group <see cref="TraceEdge" />s by a key. If they key is `null` the <see cref="TraceEdge" /> will not be grouped.
    ///     Afterwards the <paramref name="valueSelector" /> will be used to create a new <see cref="TraceEdge" />.
    /// </summary>
    /// <param name="keySelector"></param>
    /// <param name="valueSelector"></param>
    /// <typeparam name="T"></typeparam>
    public void GroupEdges<T>(Func<TraceEdgeKey, TraceEdge, T?> keySelector,
        Func<T, IEnumerable<TraceEdge>, TraceEdge> valueSelector) where T : notnull;

    /// <summary>
    ///     Group <see cref="TraceNode" />s by a key. If they key is `null` the <see cref="TraceNode" /> will not be grouped.
    ///     Afterwards the <paramref name="valueSelector" /> will be used to create a new <see cref="TraceNode" />.
    /// </summary>
    /// <param name="keySelector"></param>
    /// <param name="valueSelector"></param>
    /// <typeparam name="T"></typeparam>
    public void GroupNodes<T>(Func<TraceNodeKey, TraceNode, T?> keySelector,
        Func<T, IEnumerable<TraceNode>, TraceNode> valueSelector) where T : notnull;

    // public void AddEdge(TraceEdge traceEdge);
    // public void AddNode(TraceNode traceNode);

    // public void AddManyNodes(IEnumerable<TraceNode> nodes);
    // public void AddManyEdges(IEnumerable<TraceEdge> edges);

    // public void RemoveEdge((IPAddress, IPAddress, ushort, ushort, DataProtocol) key);
    // public void RemoveNode(IPAddress key);

    // public bool HasEdge((IPAddress, IPAddress, ushort, ushort, DataProtocol) key);
    // public bool HasNode(IPAddress key);
}

public class TraceGraph : ITraceGraph
{
    public bool RemoveDisconnectedNodes { get; set; } = true;

    public SortedList<TraceEdgeKey, TraceEdge> Edges { get; } = new(new TraceEdgeKeyComparer());
    public SortedList<TraceNodeKey, TraceNode> Nodes { get; } = new(new TraceNodeKeyComparer());

    public int EdgeCount => Edges.Count;
    public int NodeCount => Nodes.Count;

    public void FilterEdges(Func<TraceEdge, bool> filter)
    {
        var potentialOrphans = new HashSet<TraceNodeKey>();
        var removedEdges = new List<TraceEdgeKey>();

        foreach (var edge in Edges.Where(edge => !filter(edge.Value)))
        {
            removedEdges.Add(edge.Key);
            potentialOrphans.Add(edge.Value.Source);
            potentialOrphans.Add(edge.Value.Target);
        }

        foreach (var edge in removedEdges)
            Edges.Remove(edge);

        foreach (var potentialOrphan in potentialOrphans)
            RemoveNodeIfDisconnected(potentialOrphan);
    }

    public void FilterNodes(Func<TraceNode, bool> filter)
    {
        var removedNodes = Nodes
            .Where(n => !filter(n.Value))
            .Select(n => n.Key)
            .ToList();

        foreach (var node in removedNodes)
            Nodes.Remove(node);

        var removedEdges = new List<TraceEdgeKey>();
        foreach (var edge in Edges
                     .Where(e =>
                         removedNodes.Contains(e.Key.Source) ||
                         removedNodes.Contains(e.Key.Target)))
            removedEdges.Add(edge.Key);

        // TODO: potentially the graph is at an invalid state here?
        foreach (var edge in removedEdges)
            Edges.Remove(edge);
    }

    public void GroupEdges<T>(Func<TraceEdgeKey, TraceEdge, T?> keySelector,
        Func<T, IEnumerable<TraceEdge>, TraceEdge> valueSelector) where T : notnull
    {
        // Create the groups
        var dict = new Dictionary<T, List<(TraceEdgeKey, TraceEdge)>>();
        foreach (var (edgeKey, edgeValue) in Edges)
        {
            var key = keySelector(edgeKey, edgeValue);
            if (key == null)
                continue;

            if (!dict.ContainsKey(key))
                dict[key] = new List<(TraceEdgeKey, TraceEdge)>();
            dict[key].Add((edgeKey, edgeValue));
        }

        // Create the new edges and remove the old ones
        foreach (var (key, value) in dict)
        {
            // Delete all old edges
            foreach (var oldEdge in value)
                Edges.Remove(oldEdge.Item1);
            
            var newEdge = valueSelector(key, value.Select(n => n.Item2));
            AddEdge(newEdge);
        }

        foreach (var node in new List<TraceNodeKey>(Nodes.Keys))
            RemoveNodeIfDisconnected(node);
    }

    public void GroupNodes<T>(Func<TraceNodeKey, TraceNode, T?> keySelector,
        Func<T, IEnumerable<TraceNode>, TraceNode> valueSelector) where T : notnull
    {
        // Create the groups
        var dict = new Dictionary<T, List<(TraceNodeKey, TraceNode)>>();
        foreach (var (nodeKey, nodeValue) in Nodes)
        {
            var key = keySelector(nodeKey, nodeValue);
            if (key == null)
                continue;

            if (!dict.ContainsKey(key))
                dict[key] = new List<(TraceNodeKey, TraceNode)>();
            dict[key].Add((nodeKey, nodeValue));
        }

        // Create the new nodes and repoint the edges
        foreach (var (key, value) in dict)
        {
            // Create the new node
            var newNode = valueSelector(key, value.Select(n => n.Item2));
            var newNodeKey = AddNode(newNode);

            foreach (var oldNode in value)
            {
                // Delete all old nodes
                Nodes.Remove(oldNode.Item1);
                RepointEdges(oldNode.Item1, newNodeKey);
            }
        }
    }

    private TraceEdgeKey AddEdge(TraceEdge traceEdge)
    {
        var key = new TraceEdgeKey(traceEdge.Source, traceEdge.SourcePort, traceEdge.Target, traceEdge.TargetPort,
            traceEdge.DataProtocol);
        Edges.Add(
            key,
            traceEdge);
        return key;
    }

    public TraceNodeKey AddNode(TraceNode traceNode)
    {
        var key = new TraceNodeKey(traceNode.Address);
        Nodes.Add(key, traceNode);
        return key;
    }

    public void AddManyNodes(IEnumerable<TraceNode> nodes)
    {
        foreach (var node in nodes)
            AddNode(node);
    }

    public void AddManyEdges(IEnumerable<TraceEdge> edges)
    {
        foreach (var edge in edges)
            AddEdge(edge);
    }

    public void RemoveEdge(TraceEdgeKey key)
    {
        Edges.Remove(key);
        RemoveNodeIfDisconnected(key.Source);
        RemoveNodeIfDisconnected(key.Target);
    }

    public void RemoveNode(TraceNodeKey key)
    {
        Nodes.Remove(key);
    }

    public bool HasEdge(TraceEdgeKey key)
    {
        return Edges.ContainsKey(key);
    }

    public bool HasNode(TraceNodeKey key)
    {
        return Nodes.ContainsKey(key);
    }

    /// <summary>
    ///     Fill this graph with <see cref="TraceNode" />s and <see cref="TraceEdge" />s from a list of
    ///     <see cref="AggregateTrace" />s.
    /// </summary>
    /// <param name="traces"></param>
    public void FillFromTraces(List<AggregateTrace> traces)
    {
        var nodes = new Dictionary<IPAddress, TraceNode>();

        foreach (var aggregateTrace in traces)
        {
            var srcIp = new IPAddress(aggregateTrace.SourceIpBytes);
            var dstIp = new IPAddress(aggregateTrace.DestinationIpBytes);
            
            // TODO: fix the problem that arises when source dns names from the database are conflicting
            if (!nodes.ContainsKey(srcIp))
                nodes[srcIp] = new TraceNode(srcIp, srcIp.ToString(), aggregateTrace.SourceDnsName);
            
            if (!nodes.ContainsKey(dstIp))
                nodes[dstIp] = new TraceNode(dstIp, dstIp.ToString(), aggregateTrace.DestinationDnsName);
        }
        
        foreach (var node in nodes)
            AddNode(node.Value);

        var edges = traces
            .Select(trace =>
                new TraceEdge(
                    new TraceNodeKey(new IPAddress(trace.SourceIpBytes)),
                    new TraceNodeKey(new IPAddress(trace.DestinationIpBytes)),
                    trace.SourcePort,
                    trace.DestinationPort,
                    trace.DataProtocol,
                    trace.PacketCount,
                    trace.ByteCount));

        foreach (var edge in edges)
            AddEdge(edge);
    }

    private void RepointEdges(TraceNodeKey oldNodeKey, TraceNodeKey newNodeKey)
    {
        var edgesToRepoint = Edges.Where(kp =>
            kp.Value.Source.Equals(oldNodeKey) || kp.Value.Target.Equals(oldNodeKey)).ToList();

        foreach (var (oldEdgeKey, oldEdgeValue) in edgesToRepoint)
        {
            Edges.Remove(oldEdgeKey);
            
            var newEdge = new TraceEdge(
                // Swap the source and target if they are the same as the old node
                oldEdgeValue.Source.Equals(oldNodeKey) ? newNodeKey : oldEdgeValue.Source,
                oldEdgeValue.Target.Equals(oldNodeKey) ? newNodeKey : oldEdgeValue.Target,
                oldEdgeValue.SourcePort,
                oldEdgeValue.TargetPort,
                oldEdgeValue.DataProtocol,
                oldEdgeValue.PacketCount,
                oldEdgeValue.ByteCount);
            
            var key = new TraceEdgeKey(newEdge.Source, newEdge.SourcePort, newEdge.Target, newEdge.TargetPort,
                newEdge.DataProtocol);
            
            if (HasEdge(key))
            {
                Edges[key].PacketCount += newEdge.PacketCount;
                Edges[key].ByteCount += newEdge.ByteCount;
                continue;
            }

            AddEdge(newEdge);
        }
    }

    private void RemoveNodeIfDisconnected(TraceNodeKey key)
    {
        if (!RemoveDisconnectedNodes)
            return;

        var contained = Edges.Keys.Any(edge => edge.Source.Equals(key) || edge.Target.Equals(key));
        if (contained)
            return;

        Nodes.Remove(key);
    }
}