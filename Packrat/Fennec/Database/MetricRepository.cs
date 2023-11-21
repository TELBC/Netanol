using Fennec.Database.Domain;
using Fennec.Services;
using MongoDB.Driver;

namespace Fennec.Database;

/// <summary>
/// Repository for handling operations related to metrics.
/// </summary>
public interface IMetricRepository
{
    /// <summary>
    /// Gets the total count of successfully inserted entries in the db for specified time periods.
    /// </summary>
    Task GetTotalCountAsync();
}

public class CollectorDBMetrics
{
    public ulong CountTotal;
    public ulong CountLast12Hours;
    public ulong CountLast24Hours ;
    public ulong CountLast72Hours ;
}

public class MetricRepository : IMetricRepository
{
    private readonly IMongoCollection<SingleTrace> _singleTraces;
    private readonly IMetricService _metricService;
    
    public MetricRepository(IMongoCollection<SingleTrace> singleTraces, IMetricService metricService)
    {
        _singleTraces = singleTraces;
        _metricService = metricService;
    }
    
    public async Task GetTotalCountAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var twelveHoursAgo = now.AddHours(-12);
        var twentyFourHoursAgo = now.AddHours(-24);
        var seventyTwoHoursAgo = now.AddHours(-72);
        var metrics = _metricService.GetMetrics<CollectorDBMetrics>("CollectorDBMetrics");
        
        metrics.CountTotal = (ulong) await _singleTraces.CountDocumentsAsync(trace => true);
        metrics.CountLast12Hours = (ulong) await _singleTraces.CountDocumentsAsync(trace => trace.Timestamp >= twelveHoursAgo);
        metrics.CountLast24Hours = (ulong) await _singleTraces.CountDocumentsAsync(trace => trace.Timestamp >= twentyFourHoursAgo);
        metrics.CountLast72Hours = (ulong) await _singleTraces.CountDocumentsAsync(trace => trace.Timestamp >= seventyTwoHoursAgo);
    }
}