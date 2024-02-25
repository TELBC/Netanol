using Fennec.Database;
using Fennec.Database.Domain;

namespace Fennec.Processing.Graph;

/// <summary>
///     Repository to generate the graph for a given layout and timespan.
/// </summary>
public interface IGraphRepository
{
    /// <summary>
    ///     Generate the graph for a given layout within a specified timespan.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="layout"></param>
    /// <returns></returns>
    public Task<GraphDetails> GenerateGraph(DateTimeOffset from, DateTimeOffset to, Layout layout);
}

public class GraphDetails
{
    public long TotalHostCount { get; init; }
    public long TotalByteCount { get; init; }
    public long TotalPacketCount { get; init; }
    public long TotalTraceCount { get; init; }

    public List<TraceNodeDto> Nodes { get; init; } = new();
    public List<TraceEdgeDto> Edges { get; init; } = new();
}

public class GraphRepository : IGraphRepository
{
    private readonly ITraceRepository _traceRepository;
    private readonly IServiceProvider _serviceProvider;

    public GraphRepository(ITraceRepository traceRepository, IServiceProvider serviceProvider)
    {
        _traceRepository = traceRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task<GraphDetails> GenerateGraph(DateTimeOffset from, DateTimeOffset to, Layout layout)
    {
        var traces = await _traceRepository.AggregateTraces(layout.QueryConditions, from, to);
        var graph = new TraceGraph();
        graph.FillFromTraces(traces);

        foreach (var layer in layout.Layers.Where(l => l.Enabled))
            layer.Execute(graph, _serviceProvider);

        CollapseGraph(graph);
        
        var details = new GraphDetails
        {
            // TODO: rename these to the appropriate names
            // TODO: use auto mapper 
            TotalHostCount = graph.NodeCount,
            TotalTraceCount = graph.EdgeCount,
            TotalByteCount = traces.Sum(trace => (long)trace.ByteCount),
            TotalPacketCount = traces.Sum(trace => (long)trace.PacketCount),
            Nodes = graph.Nodes.Select(n => new TraceNodeDto(n.Value.Address.ToString(), n.Value.Name, n.Value.Tags)).ToList(),
            Edges = graph.Edges.Select(e => new TraceEdgeDto(
                $"{e.Value.DataProtocol}/{e.Value.Source}-{e.Value.Target}",
                e.Value.Source.ToString(),
                e.Value.Target.ToString(),
                e.Value.DataProtocol,
                e.Value.PacketCount,
                e.Value.ByteCount)).ToList()
        };

        return details;
    }

    private static void CollapseGraph(ITraceGraph graph)
    {
        graph.GroupEdges((key, _) => 
                (key.Item1, key.Item2, (ushort)0, (ushort)0, key.Item5),
            (key, value) =>
            {
                var traceEdges = value.ToList();
                return new TraceEdge(key.Item1, key.Item2, 0, 0, key.Item5,
                    (ulong)traceEdges.Sum(t => (long)t.PacketCount),
                    (ulong)traceEdges.Sum(t => (long)t.ByteCount));
            });
    }
}