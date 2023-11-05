using System.Diagnostics;
using System.Net;
using Fennec.Collectors;
using Fennec.Database;
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
    IDictionary<string, LayoutNodeDto> Nodes,
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

// [Authorize]
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
            .ToDictionary(n => n.Id, n => n);

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
        /*
        // TODO: build in protection mode and prevent the client from loading more than like 10 000 hosts
        // somehow establish how those hosts are represented in the given layout?

        var watch = Stopwatch.StartNew();

        // get the layout & include the hosts in the graph nodes
        var layout = await _context.Layouts
            .Include(layout => layout.GraphNodes)
            .FirstOrDefaultAsync(layout => layout.Name == name);

        if (layout == null)
            return NotFound("The given layout was not found.");

        // this looks like multiple queries but it isn't because of deferred execution
        var rootQuery = _context.SingleTraces
            .Where(trace => trace.Timestamp >= request.From.ToOffset(TimeSpan.Zero) && trace.Timestamp <= request.To.ToOffset(TimeSpan.Zero))
            .GroupBy(trace => new { trace.SourceHostId, trace.DestinationHostId });
        var sourceQuery = rootQuery
            .Select(a => a.Key.SourceHostId)
            .Distinct();
        var destinationQuery = rootQuery
            .Select(a => a.Key.DestinationHostId).Distinct();
        var unionQuery = sourceQuery.Union(destinationQuery);
        var joinedQuery = unionQuery
            .Join(_context.NetworkHosts, id => id, host => host.Id, (_, host) => host);
        var filteredQuery = joinedQuery
            .Where(networkHost => _context.HostNodes
                .Where(hostNode => hostNode.LayoutId == layout.Id)
                .All(hostNode => hostNode.NetworkHostId != networkHost.Id));
        var unknownHosts = await filteredQuery.ToListAsync();

        if (unknownHosts.Count != 0)
        {
            // create graph nodes for those hosts
            var graphNodes = unknownHosts
                .Select(host =>
                    host.DnsInfo is { DnsName: { } dnsName }
                        ? new HostNode(layout, dnsName, host)
                        : new HostNode(layout, host.IpAddress.ToString(), host))
                .ToList();

            _context.GraphNodes.AddRange(graphNodes);
            await _context.SaveChangesAsync();
        }

        // build the translation table from network host id to graph node id
        var translationTable = new Dictionary<long, long>();
        foreach (var networkHost in await joinedQuery.ToListAsync())
        {
            var graphHostNode = layout.GraphNodes
                .OfType<HostNode>()
                .Where(h => h.IsVisible && h.LayoutId == layout.Id)
                .FirstOrDefault(h => h.Id == networkHost.Id);

            if (graphHostNode != null)
            {
                translationTable.Add(networkHost.Id, graphHostNode.Id);
                continue;
            }

            var cg = await _context.CompressedGroups
                .FirstOrDefaultAsync(cg => cg.GraphNode.LayoutId == layout.Id &&
                                           cg.NetworkHostId == networkHost.Id);

            if (cg == null)
                continue;

            translationTable.Add(networkHost.Id, cg.GraphNodeId);
        }

        // translate the traces & aggregate them again on the graph node ids
        var traces = (await rootQuery.ToListAsync())
            .Select(grouping => new
            {
                SourceGraphNodeId = translationTable[grouping.Key.SourceHostId],
                DestinationGraphNodeId = translationTable[grouping.Key.DestinationHostId],
                PacketCount = grouping.Sum(st => (long) st.PacketCount),
                ByteCount = grouping.Sum(st => (long) st.ByteCount)
            })
            .GroupBy(t => new { t.SourceGraphNodeId, t.DestinationGraphNodeId })
            .Select(group => new AggregatedTraceDto(
                group.Key.SourceGraphNodeId,
                group.Key.DestinationGraphNodeId,
                group.Sum(trace => trace.PacketCount),
                group.Sum(trace => trace.ByteCount),
                group.Count()))
            .ToList();

        watch.Stop();
        return Ok(new GraphResponse(
            null!,
            new RequestStatistics(unknownHosts.Count, watch.Elapsed),
            layout.GraphNodes
                .Where(g => g.IsVisible)
                .ToDictionary(g => g.Id, g => new GraphNodeDto(g.Id, g.DisplayName)),
            traces));
            */
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
        /*
        IPAddress.TryParse(request.Subnet, out var subnet);
        if (subnet == null)
            return BadRequest("Subnet is not a valid IP address.");

        IPAddress.TryParse(request.SubnetMask, out var subnetMask);
        if (subnetMask == null)
            return BadRequest("Subnet mask is not a valid IP address.");

        if (request.GroupType == GroupType.Island)
            return BadRequest("Island grouping is not yet implemented.");

        // get the layout and make sure it exists
        var layout = await _context.Layouts
            .Include(layout => layout.GraphNodes)
            .FirstOrDefaultAsync(layout => layout.Name == name);

        if (layout == null)
            return NotFound("The given layout was not found.");

        // get corresponding hosts
        var layoutHostNodes = await _context.GraphNodes
            .OfType<HostNode>()
            .Include(n => n.NetworkHost)
            .Where(n => n.LayoutId == layout.Id && n.IsVisible)
            .ToListAsync();

        // check if a host is in the given subnet
        bool IsInSubnet(IPAddress address, IPAddress subnet, IPAddress mask)
        {
            var addressBytes = address.GetAddressBytes();
            var subnetBytes = subnet.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();

            for (var i = 0; i < addressBytes.Length; i++)
            {
                // Apply the subnet mask by performing a bitwise AND between the address and the mask
                addressBytes[i] = (byte)(addressBytes[i] & maskBytes[i]);
                subnetBytes[i] = (byte)(subnetBytes[i] & maskBytes[i]);

                // Compare the masked addresses
                if (addressBytes[i] != subnetBytes[i]) return false;
            }

            return true;
        }

        // get the hosts afflicted by the subnet and subnet mask
        var matchingHostNodes = layoutHostNodes
            .Where(h => h.IsVisible && IsInSubnet(h.NetworkHost.IpAddress, subnet, subnetMask))
            .Select(h =>
            {
                h.IsVisible = false;
                return h;
            })
            .ToList();

        if (matchingHostNodes.Count == 0)
            return BadRequest("No hosts found in the given subnet.");

        // create a new graph node
        var groupNode = new GraphNode(layout, $"{request.Subnet}/{request.SubnetMask}");
        _context.GraphNodes.Add(groupNode);

        // create compressed group entries
        foreach (var compressedGroup in matchingHostNodes.Select(hostNode =>
                     new CompressedGroup(groupNode, hostNode.NetworkHost)))
            _context.CompressedGroups.Add(compressedGroup);

        // save changes
        await _context.SaveChangesAsync();

        // TODO: return statistics
        return Ok();
        */
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
        /*
        // get the layout and make sure it exists
        var layout = await _context.Layouts
            .Include(layout => layout.GraphNodes)
            .FirstOrDefaultAsync(layout => layout.Name == name);
        if (layout == null) return NotFound("The given layout was not found.");

        var groupNode = await _context.GraphNodes
            .FirstOrDefaultAsync(g => g.Id == groupId);
        if (groupNode == null) return NotFound("The given group node was not found.");

        var compressedGroupNodes = await _context.CompressedGroups
            .Where(cg => cg.GraphNodeId == groupId)
            .ToListAsync();
        if (compressedGroupNodes.Count == 0) return BadRequest("The given group node is not a compressed group.");

        var hostNodes = await _context.GraphNodes
            .OfType<HostNode>()
            .Where(hn => hn.LayoutId == layout.Id)
            .ToListAsync();

        foreach (var compressedGroupNode in compressedGroupNodes)
        {
            var correspondingHostNode = hostNodes
                .FirstOrDefault(hn => hn.NetworkHostId == compressedGroupNode.NetworkHostId);
            if (correspondingHostNode == null) continue;

            correspondingHostNode.IsVisible = true;
            _context.CompressedGroups.Remove(compressedGroupNode);
        }

        _context.GraphNodes.Remove(groupNode);
        await _context.SaveChangesAsync();

        // TODO: return statistics
        return Ok();
        */
        return Ok();
    }
}
