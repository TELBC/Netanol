using Microsoft.EntityFrameworkCore;
using Tapas.Database.Dto;
using Tapas.Models;

namespace Tapas.Database;

public class TraceRepository
{
    private readonly TapasContext _context;

    public TraceRepository(TapasContext context)
    {
        _context = context;
    }

    public async Task AddSingleTrace(SingleTrace singleTrace)
    {
        _context.SingleTraces.Add(singleTrace);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<SingleTraceDto>> GroupTracesByTimeSpanAndReturnAsDto(DateTimeOffset from, DateTimeOffset until)
    {
        return await _context.SingleTraces.Where(trace => trace.Timestamp >= from && trace.Timestamp <= until).GroupBy(st => new
            {
                st.Protocol,
                st.SourceIpAddress,
                st.SourcePort,
                st.DestinationIpAddress,
                st.DestinationPort
            })
            .Select(group => new SingleTraceDto(
                group.Key.Protocol.ToString(),
                group.Key.SourceIpAddress.ToString(),
                group.Key.SourcePort,
                group.Key.DestinationIpAddress.ToString(),
                group.Key.DestinationPort,
                group.Count()
            ))
            .ToListAsync();
    }
}
