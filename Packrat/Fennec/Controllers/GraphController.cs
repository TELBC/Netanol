using System.Diagnostics;
using System.Net;
using Fennec.Collectors;
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

public record GroupRequest(GroupType GroupType, string Subnet, string SubnetMask);

public record GraphStatistics(long TotalHostCount, long TotalByteCount, long TotalPacketCount, long TotalTraceCount);

public record RequestStatistics(long NewHostCount, TimeSpan ProcessingTime);

// TODO: rename to SourceId and DestinationId
public record LayoutEdgeDto(string SourceHostId, string DestinationHostId, ulong PacketCount, ulong ByteCount, ulong TraceCount)
{
    public LayoutEdgeDto(IPAddress sourceIp, IPAddress destinationIp, ulong packetCount, ulong byteCount, ulong traceCount)
        : this(sourceIp.ToString(), destinationIp.ToString(), packetCount, byteCount, traceCount) { }
}

public record LayoutNodeDto(string Id, string DisplayName)
{
    public LayoutNodeDto(IPAddress ipAddress)
        : this(ipAddress.ToString(), ipAddress.ToString()) { }
}

public record GraphResponse(
    GraphStatistics GraphStatistics,
    RequestStatistics RequestStatistics,
    List<LayoutNodeDto> Nodes,
    List<LayoutEdgeDto> Edges);

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
[Route("graph/{name}")]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Generate Graphs")]
public class GraphController : ControllerBase
{
    private readonly ITraceRepository _traceRepository;

    public GraphController(ITraceRepository traceRepository)
    {
        _traceRepository = traceRepository;
    }

    /// <summary>
    /// Generate the graph for a given layout within a specified timespan. Unknown nodes are added during this process.
    /// </summary>
    /// <remarks>
    /// This endpoint fetches the graph layout, adds new nodes that were previously unknown, and returns the graph with its associated traces.
    /// </remarks>
    /// <param name="name">The name of the layout.</param>
    /// <param name="request">A JSON body containing the timespan for which the graph is requested.</param>
    /// <returns>An object containing the graph layout and its associated traces.</returns>
    /// <response code="200">Successfully returned the graph layout.</response>
    /// <response code="404">The layout specified by the name was not found.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateGraph(string name, [FromBody] GraphRequest request)
    {
        var edges = await _traceRepository.AggregateTraces(request.From, request.To);

        var nodes = edges
            .SelectMany<AggregateTrace, byte[]>(trace => new[] { trace.SourceIpBytes, trace.DestinationIpBytes })
            .GroupBy(t => t, new ByteArrayComparer())
            .Select(t => t.First())
            .Select(bytes => new LayoutNodeDto(new IPAddress(bytes)))
            .ToList();

        var dtoEdges = edges.Select(trace => 
            new LayoutEdgeDto(
                new IPAddress(trace.SourceIpBytes), 
                new IPAddress(trace.DestinationIpBytes), 
                trace.PacketCount, 
                trace.ByteCount, 
                0))
            .ToList();
        
        var response = new GraphResponse(
            null!,
            null!,
            nodes,
            dtoEdges);
        
        return Ok(response);
   }

    /// <summary>
    /// Groups nodes in a graph by the specified subnet and subnet mask.
    /// </summary>
    /// <remarks>
    /// This endpoint allows you to create a new group within a layout based on a subnet and a subnet mask.
    /// The nodes within the subnet will be grouped together and a new graph node will be created to represent them.
    /// </remarks>
    /// <param name="name">The name of the layout where the group will be created.</param>
    /// <param name="request">A JSON body containing the subnet, subnet mask, and group type for the new group.</param>
    /// <returns>Returns an object with details of the newly created group.</returns>
    /// <response code="200">The group was successfully created.</response>
    /// <response code="400">The subnet or subnet mask was invalid, or the group type is not supported.</response>
    /// <response code="404">The given layout was not found.</response>
    [HttpPost("group")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateGroup(string name, [FromBody] GroupRequest request)
    {
       return Ok();
    }

    
    /// <summary>
    ///     Dissolves a group within a specific layout.
    /// </summary>
    /// <remarks>
    ///     This API removes a group node from a layout based on its ID and makes any corresponding host nodes visible.
    /// </remarks>
    /// <param name="name">The name of the layout.</param>
    /// <param name="groupId">The ID of the group node to be dissolved.</param>
    /// <returns>Returns Ok if the group is successfully dissolved.</returns>
    /// <response code="200">The group was successfully dissolved.</response>
    /// <response code="400">The given group node is not a compressed group.</response>
    /// <response code="404">The given layout or group node was not found.</response>
    [HttpDelete("group/{groupId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DissolveGroup(string name, long groupId)
    {
       return Ok();
    }
}
