using Fennec.Database.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Fennec.Services;

public class MetricService
{
    private readonly IMongoCollection<SingleTrace> _singleTraces;
    private readonly IMongoClient _mongoClient;

    public MetricService(IMongoCollection<SingleTrace> singleTraces, IMongoClient mongoClient)
    {
        _singleTraces = singleTraces;
        _mongoClient = mongoClient;
    }
    
    public async Task<(long, BsonDocument)> GetAllMetrics()
    {
        var count = await _singleTraces.CountDocumentsAsync(trace => true);
        var byteCountSum = await _singleTraces.Aggregate()
            .Group(new BsonDocument { { "_id", 1 }, { "totalByteCount", new BsonDocument { { "$sum", "$byteCount" } } } })
            .FirstOrDefaultAsync();

        return (count, byteCountSum);
    }

}
