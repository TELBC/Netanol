using Fennec.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Controllers;

public record NodeDto(long Id, string IpAddress);

public record EdgeDto(long SourceHostId, long DestinationHostId, int Count);

public record GraphDto(IDictionary<long, NodeDto> Nodes, List<EdgeDto> Edges);

[Route("traces")]
public class TraceController : ControllerBase
{
    private readonly TapasContext _context;

    public TraceController(TapasContext context)
    {
        _context = context;
    }

    [HttpGet("get_by_window")]
    public async Task<IActionResult> GetTracesByWindow([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
    {
        var edges = await _context.SingleTraces
            .Where(trace => trace.Timestamp >= from && trace.Timestamp <= to)
            .GroupBy(trace => new { trace.SourceHostId, trace.DestinationHostId })
            .Select(trace => new EdgeDto(trace.Key.SourceHostId, trace.Key.DestinationHostId, trace.Count()))
            .ToListAsync();

        var hostIds = edges.Select(t => t.SourceHostId).Concat(edges.Select(t => t.DestinationHostId)).Distinct();
        var nodes = await _context.NetworkHosts
            .Where(host => hostIds.Contains(host.Id))
            .Select(host => new NodeDto(host.Id, host.IpAddress.ToString()))
            .ToDictionaryAsync(host => host.Id);

        return Ok(new GraphDto(nodes, edges));
    }
}