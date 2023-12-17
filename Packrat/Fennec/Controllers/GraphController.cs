using System.Net;
using Fennec.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fennec.Controllers;

public record GraphRequest(DateTimeOffset From, DateTimeOffset To);

public enum GroupType
{
    Compressed,
    Island
}

public record GraphStatistics(long TotalHostCount, long TotalByteCount, long TotalPacketCount, long TotalTraceCount);

public record RequestStatistics(long NewHostCount, TimeSpan ProcessingTime);

// TODO: rename to SourceId and DestinationId
public record LayoutEdgeDto(string Source, string Target, ulong PacketCount, ulong ByteCount, ulong TraceCount)
{
    public LayoutEdgeDto(IPAddress sourceIp, IPAddress destinationIp, ulong packetCount, ulong byteCount, ulong traceCount)
        : this(sourceIp.ToString(), destinationIp.ToString(), packetCount, byteCount, traceCount) { }
}

public record LayoutNodeDto(string Id, string Name)
{
    public LayoutNodeDto(IPAddress ipAddress)
        : this(ipAddress.ToString(), ipAddress.ToString()) { }
}

public record GraphResponse(
    GraphStatistics GraphStatistics,
    RequestStatistics RequestStatistics,
    Dictionary<string, LayoutNodeDto> Nodes,
    Dictionary<string, LayoutEdgeDto> Edges);

public class ByteArrayComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        return x.SequenceEqual(y);
    }

    public int GetHashCode(byte[]? obj)
    {
        if (obj == null) 
            return 0;
        
        unchecked
        {
            return obj.Aggregate(17, (current, val) => current * 31 + val);
        }
    }
}

[Authorize]
[Route("graph/{layoutName}")]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Generate Graphs")]
public class GraphController : ControllerBase
{
    private readonly ITraceRepository _traceRepository;
    private readonly ILayoutRepository _layoutRepository;

    public GraphController(ITraceRepository traceRepository, ILayoutRepository layoutRepository)
    {
        _traceRepository = traceRepository;
        _layoutRepository = layoutRepository;
    }

    /// <summary>
    /// Generate the graph for a given layout within a specified timespan.
    /// </summary>
    /// <param name="layoutName">The name of the layout.</param>
    /// <param name="request">A JSON body containing the timespan for which the graph is requested.</param>
    /// <returns>An object containing the graph layout and its associated traces.</returns>
    /// <response code="200">Successfully returned the graph layout.</response>
    /// <response code="404">The layout specified by the name was not found.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateGraph(string layoutName, [FromBody] GraphRequest request)
    {
        var layout = await _layoutRepository.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");
        
        var edges = await _traceRepository.AggregateTraces(request.From, request.To);

        var nodes = edges
            .SelectMany<AggregateTrace, byte[]>(trace => new[] { trace.SourceIpBytes, trace.DestinationIpBytes })
            .GroupBy(t => t, new ByteArrayComparer())
            .Select(t => t.First())
            .Select(bytes => new LayoutNodeDto(new IPAddress(bytes)))
            .ToDictionary(dto => dto.Id, dto => dto);

        var dtoEdges = edges.Select(trace => 
            new LayoutEdgeDto(
                new IPAddress(trace.SourceIpBytes), 
                new IPAddress(trace.DestinationIpBytes), 
                trace.PacketCount, 
                trace.ByteCount, 
                0))
            .ToDictionary(dto => $"{dto.Source}-{dto.Target}", dto => dto);

        var totalPackets = dtoEdges.Sum(edge => (int)edge.Value.PacketCount);
        var totalByteCount = dtoEdges.Sum(edge => (int)edge.Value.ByteCount);

        var response = new GraphResponse(
            new GraphStatistics(nodes.Count,totalByteCount, totalPackets, dtoEdges.Count),
            null!,
            nodes,
            dtoEdges);
        
        return Ok(response);
    }
}
