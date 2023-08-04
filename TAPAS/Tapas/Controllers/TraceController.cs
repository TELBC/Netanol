using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tapas.Database;

namespace Tapas.Controllers;

public class NetworkHostDto
{
    public long Id { get; set; }
    public string IpAddress { get; set; }
}


[Route("traces")]
public class TraceController : ControllerBase
{
    private readonly TraceRepository _traceRepository;
    private readonly TapasContext _context;
    private readonly IMapper _mapper;

    public TraceController(TraceRepository traceRepository, IMapper mapper, TapasContext context)
    {
        _traceRepository = traceRepository;
        _mapper = mapper;
        _context = context;
    }
    
    [HttpGet("get_by_window")]
    public async Task<IActionResult> GetTracesByWindow([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
    {
        var traces = await _context.SingleTraces
            .Where(trace => trace.Timestamp >= from && trace.Timestamp <= to)
            .GroupBy(trace => new { trace.SourceHostId, trace.DestinationHostId })
            .Select(trace => new
            {
                trace.Key.SourceHostId,
                trace.Key.DestinationHostId,
                Count = trace.Count()
            })
            .ToListAsync();

        var hostIds = traces.Select(t => t.SourceHostId).Concat(traces.Select(t => t.DestinationHostId)).Distinct();
        var nodes = await _context.NetworkHosts
            .Where(host => hostIds.Contains(host.Id))
            .Select(host => new NetworkHostDto
            {
                Id = host.Id,
                IpAddress = host.IpAddress.ToString()
            })
            .ToDictionaryAsync(host => host.Id);
        
        return Ok(new
        {
            Nodes = nodes,
            Traces = traces
        });
    }
}