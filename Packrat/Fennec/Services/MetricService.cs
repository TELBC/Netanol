using Fennec.Database.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Fennec.Services;
/// <summary>
/// Provides Metrics to frontend.
/// </summary>
public record Metrics(ulong CountTotal, ulong CountLast12Hours, ulong CountLast24Hours, ulong CountLast72Hours);

public interface IMetricService
{
    Task<Metrics> GetAllMetrics();
}
public class MetricService : IMetricService
{
    private readonly IMongoCollection<SingleTrace> _singleTraces;

    public MetricService(IMongoCollection<SingleTrace> singleTraces)
    {
        _singleTraces = singleTraces;
    }
    
    public async Task<Metrics> GetAllMetrics()
    {
        var now = DateTimeOffset.UtcNow;
        var twelveHoursAgo = now.AddHours(-12);
        var twentyFourHoursAgo = now.AddHours(-24);
        var seventyTwoHoursAgo = now.AddHours(-72);

        var countTotal = (ulong)await _singleTraces.CountDocumentsAsync(trace => true);
        var countLast12Hours = (ulong)await _singleTraces.CountDocumentsAsync(trace => trace.Timestamp >= twelveHoursAgo);
        var countLast24Hours = (ulong)await _singleTraces.CountDocumentsAsync(trace => trace.Timestamp >= twentyFourHoursAgo);
        var countLast72Hours = (ulong)await _singleTraces.CountDocumentsAsync(trace => trace.Timestamp >= seventyTwoHoursAgo);
        
        return new Metrics(countTotal, countLast12Hours, countLast24Hours, countLast72Hours);
    }
}
