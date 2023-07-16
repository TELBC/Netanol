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

    public Task<IEnumerable<SingleTrace>> GetAllSingleTraces()
    {
        return Task.FromResult<IEnumerable<SingleTrace>>(_context.SingleTraces);
    }
}