using System.Net;
using Fennec.Database.Domain;
using Fennec.Utils;

namespace Fennec.Database.Graph;

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

public class GraphRepository : IGraphRepository
{
    private readonly ITraceRepository _traceRepository;

    public GraphRepository(ITraceRepository traceRepository)
    {
        _traceRepository = traceRepository;
    }

    public async Task<GraphDetails> GenerateGraph(DateTimeOffset from, DateTimeOffset to, Layout layout)
    {
        var traces = await _traceRepository.AggregateTraces(from, to);
        foreach (var layer in layout.Layers)
            layer.Execute(ref traces);

        // TODO: Temporary solution to avoid duplication
        traces = CollapseTraces(traces);

        var nodes = traces
            .SelectMany<AggregateTrace, byte[]>(trace => new[] { trace.SourceIpBytes, trace.DestinationIpBytes })
            .GroupBy(t => t, new ByteArrayComparer())
            .Select(t => t.First())
            .Select(bytes => new Node(new IPAddress(bytes).ToString(), new IPAddress(bytes).ToString()))
            .ToDictionary(dto => dto.Id, dto => dto);

        var edges = traces
            .Select(trace =>
                new Edge(
                    new IPAddress(trace.SourceIpBytes).ToString(),
                    new IPAddress(trace.DestinationIpBytes).ToString(),
                    trace.PacketCount,
                    trace.ByteCount))
            .ToDictionary(dto => $"{dto.Source}-{dto.Target}", dto => dto);

        var details = new GraphDetails
        {
            TotalHostCount = nodes.Count,
            TotalByteCount = traces.Sum(trace => (long)trace.ByteCount),
            TotalPacketCount = traces.Sum(trace => (long)trace.PacketCount),
            TotalTraceCount = traces.Count,
            Nodes = nodes,
            Edges = edges
        };

        return details;
    }

    private static List<AggregateTrace> CollapseTraces(IEnumerable<AggregateTrace> edges)
    {
        return edges.GroupBy(t => (t.SourceIpBytes, t.DestinationIpBytes), (key, value) =>
        {
            return new AggregateTrace
            {
                SourceIpBytes = key.Item1,
                DestinationIpBytes = key.Item2,
                PacketCount = (ulong) value.Sum(t => (long)t.PacketCount),
                ByteCount = (ulong) value.Sum(trace => (long)trace.ByteCount)
            };
        }, new IpPairComparer())
        .ToList();
    }
}