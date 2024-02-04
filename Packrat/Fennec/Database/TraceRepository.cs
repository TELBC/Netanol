using System.Net;
using Fennec.Database.Domain;
using Fennec.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Fennec.Database;

/// <summary>
/// Database operation abstractions for handling traces.
/// </summary>
public interface ITraceRepository
{
    /// <summary>
    /// Add a single trace to the database.
    /// </summary>
    /// <param name="singleTrace"></param>
    /// <returns></returns>
    public Task AddSingleTrace(SingleTrace singleTrace);
    
    public Task ImportTraceImportInfo(IEnumerable<TraceImportInfo> traceImportInfos);

    /// <summary>
    /// Aggregate all traces in the database by their source and destination <see cref="IPAddress"/> and port.
    /// </summary>
    /// <returns></returns>
    public Task<List<AggregateTrace>> AggregateTraces(DateTimeOffset start, DateTimeOffset end);
    
    /// <summary>
    /// Get all traces from the database.
    /// </summary>
    /// <returns></returns>
    public Task<List<SingleTrace>> GetAllTraces();
    
    /// <summary>
    /// Update a trace in the database.
    /// </summary>
    /// <param name="trace"></param>
    /// <returns></returns>
    public Task UpdateTrace(SingleTrace trace);
}

public record TraceImportInfo(
    DateTimeOffset ReadTime, IPAddress ExporterIp,
    IPAddress SrcIp, ushort SrcPort,
    IPAddress DstIp, ushort DstPort,
    ulong PacketCount, ulong ByteCount, TraceProtocol Protocol);

public class AggregateTrace
{
    public AggregateTrace(byte[] sourceIpBytes, byte[] destinationIpBytes, ushort sourcePort, ushort destinationPort, TraceProtocol protocol, ulong packetCount, ulong byteCount)
    {
        SourceIpBytes = sourceIpBytes;
        DestinationIpBytes = destinationIpBytes;
        SourcePort = sourcePort;
        DestinationPort = destinationPort;
        Protocol = protocol;
        PacketCount = packetCount;
        ByteCount = byteCount;
    }

#pragma warning disable CS8618
    public AggregateTrace() { }
#pragma warning restore CS8618
    
    [BsonElement("sourceIpBytes")]
    public byte[] SourceIpBytes { get; set; }
    
    [BsonElement("destinationIpBytes")]
    public byte[] DestinationIpBytes { get; set; }
    
    [BsonElement("sourcePort")]
    public ushort SourcePort { get; set; }
    
    [BsonElement("destinationPort")]
    public ushort DestinationPort { get; set; }
    
    [BsonElement("protocol")]
    public TraceProtocol Protocol { get; set; }
    
    [BsonElement("packetCount")]
    public ulong PacketCount { get; set; }
    
    [BsonElement("byteCount")]
    public ulong ByteCount { get; set; }
}

public class TraceRepository : ITraceRepository
{
    private readonly IMongoCollection<SingleTrace> _traces;
    private readonly DnsResolverService _dnsResolverService;

    public TraceRepository(IMongoDatabase database, DnsResolverService dnsResolverService)
    {
        _dnsResolverService = dnsResolverService;
        _traces = database.GetCollection<SingleTrace>("singleTraces");
    }

    public async Task AddSingleTrace(SingleTrace singleTrace)
    {
        await _traces.InsertOneAsync(singleTrace);
    }

    /// <summary>
    /// Import a list of <see cref="TraceImportInfo"/> into the database.
    /// </summary>
    /// <param name="traceImportInfos"></param>
    public async Task ImportTraceImportInfo(IEnumerable<TraceImportInfo> traceImportInfos)
    {
        var tasks = traceImportInfos.Select(async traceImportInfo =>
        {
            var srcDnsEntryTask = _dnsResolverService.GetDnsEntryFromCacheOrResolve(traceImportInfo.SrcIp);
            var dstDnsEntryTask = _dnsResolverService.GetDnsEntryFromCacheOrResolve(traceImportInfo.DstIp);

            await Task.WhenAll(srcDnsEntryTask, dstDnsEntryTask);

            var srcDnsEntry = srcDnsEntryTask.Result;
            var dstDnsEntry = dstDnsEntryTask.Result;

            var singleTrace = new SingleTrace
            {
                Timestamp = traceImportInfo.ReadTime,
                Protocol = traceImportInfo.Protocol,
                Source = new SingleTraceEndpoint(traceImportInfo.SrcIp, traceImportInfo.SrcPort, srcDnsEntry),
                Destination = new SingleTraceEndpoint(traceImportInfo.DstIp, traceImportInfo.DstPort, dstDnsEntry),
                ByteCount = traceImportInfo.ByteCount,
                PacketCount = traceImportInfo.PacketCount
            };

            await AddSingleTrace(singleTrace);
        });

        await Task.WhenAll(tasks);
    }

    public async Task<List<AggregateTrace>> AggregateTraces(DateTimeOffset start, DateTimeOffset end)
    {
        return await _traces.Aggregate()
            .Group(
                new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "sourceIp", "$source.ipBytes" },
                            { "sourcePort", "$source.port" },
                            { "destinationIp", "$destination.ipBytes" },
                            { "destinationPort", "$destination.port" },
                            { "protocol", "$protocol" }
                        }
                    },
                    { "totalBytes", new BsonDocument("$sum", "$byteCount") },
                    { "totalPackets", new BsonDocument("$sum", "$packetCount") }
                }
            )
            .Project<AggregateTrace>(
                new BsonDocument
                {
                    { "_id", 0 },
                    { "sourceIpBytes", "$_id.sourceIp" },
                    { "sourcePort", "$_id.sourcePort" },
                    { "destinationIpBytes", "$_id.destinationIp" },
                    { "destinationPort", "$_id.destinationPort" },
                    { "protocol", "$_id.protocol" },
                    { "byteCount", "$totalBytes" },
                    { "packetCount", "$totalPackets" }
                }
            ).ToListAsync();
    }
    
    /// <summary>
    /// Get all traces from the database.
    /// </summary>
    /// <returns></returns>
    public async Task<List<SingleTrace>> GetAllTraces()
    {
        return await _traces.Find(_ => true).ToListAsync();
    }
    
    /// <summary>
    /// Update a trace in the database.
    /// </summary>
    /// <param name="trace"></param>
    public async Task UpdateTrace(SingleTrace trace)
    {
        var filter = Builders<SingleTrace>.Filter.Eq(s => s.Id, trace.Id);
        await _traces.ReplaceOneAsync(filter, trace);
    }
}