using System.Net;
using Fennec.Database.Domain;
using Fennec.Parsers;
using Fennec.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Fennec.Database;

/// <summary>
///     Database operation abstractions for handling traces.
/// </summary>
public interface ITraceRepository
{
    public Task ImportTraceImportInfo(IEnumerable<TraceImportInfo> traceImportInfos);

    /// <summary>
    ///     Aggregate all traces in the database by their source and destination <see cref="IPAddress" /> and port.
    /// </summary>
    /// <returns></returns>
    public Task<List<AggregateTrace>> AggregateTraces(QueryConditions conditions, DateTimeOffset start,
        DateTimeOffset end);
}

public record TraceImportInfo(
    DateTimeOffset ReadTime,
    IPAddress ExporterIp,
    IPAddress SrcIp,
    ushort SrcPort,
    IPAddress DstIp,
    ushort DstPort,
    bool Duplicate,
    ulong PacketCount,
    ulong ByteCount,
    DataProtocol DataProtocol,
    FlowProtocol FlowProtocol)
{
    public bool Duplicate { get; set; } = Duplicate;
}

public class AggregateTrace
{
    public AggregateTrace(byte[] sourceIpBytes, byte[] destinationIpBytes, ushort sourcePort, ushort destinationPort,
        DataProtocol protocol, ulong packetCount, ulong byteCount)
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
    public AggregateTrace()
    {
    }
#pragma warning restore CS8618

    [BsonElement("sourceIpBytes")] public byte[] SourceIpBytes { get; set; }

    [BsonElement("destinationIpBytes")] public byte[] DestinationIpBytes { get; set; }

    [BsonElement("sourcePort")] public ushort SourcePort { get; set; }

    [BsonElement("destinationPort")] public ushort DestinationPort { get; set; }

    [BsonElement("protocol")] public DataProtocol Protocol { get; set; }

    [BsonElement("packetCount")] public ulong PacketCount { get; set; }

    [BsonElement("byteCount")] public ulong ByteCount { get; set; }
}

public class TraceRepository : ITraceRepository
{
    private readonly DnsResolverService _dnsService;
    private readonly IDuplicateFlaggingService _flaggingService;
    private readonly IMongoCollection<SingleTrace> _traces;

    public TraceRepository(IMongoDatabase database, DnsResolverService dnsService,
        IDuplicateFlaggingService flaggingService)
    {
        _dnsService = dnsService;
        _flaggingService = flaggingService;
        _traces = database.GetCollection<SingleTrace>("singleTraces");
    }

    /// <summary>
    ///     Import a list of <see cref="TraceImportInfo" /> into the database.
    /// </summary>
    /// <param name="traceImportInfos"></param>
    public async Task ImportTraceImportInfo(IEnumerable<TraceImportInfo> traceImportInfos)
    {
        var tasks = traceImportInfos.Select(async traceImportInfo =>
        {
            _flaggingService.FlagTrace(traceImportInfo);
            var srcDns = await _dnsService.GetDnsEntryFromCacheOrResolve(traceImportInfo.SrcIp);
            var dstDns = await _dnsService.GetDnsEntryFromCacheOrResolve(traceImportInfo.DstIp);

            var singleTrace = new SingleTrace
            {
                Timestamp = traceImportInfo.ReadTime,
                DataProtocol = traceImportInfo.DataProtocol,
                FlowProtocol = traceImportInfo.FlowProtocol,
                Source = new SingleTraceEndpoint(traceImportInfo.SrcIp, traceImportInfo.SrcPort, srcDns),
                Destination = new SingleTraceEndpoint(traceImportInfo.DstIp, traceImportInfo.DstPort, dstDns),
                Duplicate = traceImportInfo.Duplicate,
                ByteCount = traceImportInfo.ByteCount,
                PacketCount = traceImportInfo.PacketCount
            };

            return _traces.InsertOneAsync(singleTrace);
        });

        await Task.WhenAll(tasks);
    }

    public async Task<List<AggregateTrace>> AggregateTraces(QueryConditions conditions, DateTimeOffset start,
        DateTimeOffset end)
    {
        var filter = BuildQueryConditions(conditions);
        return await _traces 
            .Aggregate()
            .Match(t => t.Timestamp >= start && t.Timestamp <= end)
            .Match(filter)
            .Group(
                new BsonDocument
                {
                    {
                        "_id", new BsonDocument
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

    private static FilterDefinition<SingleTrace> BuildQueryConditions(QueryConditions conditions)
    {
        var filters = new List<FilterDefinition<SingleTrace>>();

        if (conditions.AllowDuplicates.HasValue)
            filters.Add(Builders<SingleTrace>.Filter.Eq(x => x.Duplicate, conditions.AllowDuplicates.Value));

        if (conditions.FlowProtocolsWhitelist != null) // && conditions.FlowProtocolsWhitelist.Any())
            filters.Add(Builders<SingleTrace>.Filter.In(x => x.FlowProtocol, conditions.FlowProtocolsWhitelist));

        if (conditions.DataProtocolsWhitelist != null) // && conditions.DataProtocolsWhitelist.Any())
            filters.Add(Builders<SingleTrace>.Filter.In(x => x.DataProtocol, conditions.DataProtocolsWhitelist));

        if (conditions.PortsWhitelist == null) // || !conditions.PortsWhitelist.Any())
            return filters.Any() ? Builders<SingleTrace>.Filter.And(filters) : Builders<SingleTrace>.Filter.Empty;
        
        var portFilters = new List<FilterDefinition<SingleTrace>>();
        foreach (var port in conditions.PortsWhitelist)
        {
            var sourcePortFilter = Builders<SingleTrace>.Filter.Eq(x => x.Source.Port, port);
            var destinationPortFilter = Builders<SingleTrace>.Filter.Eq(x => x.Destination.Port, port);
            portFilters.Add(sourcePortFilter | destinationPortFilter);
        }

        filters.Add(Builders<SingleTrace>.Filter.Or(portFilters));
        return filters.Any() ? Builders<SingleTrace>.Filter.And(filters) : Builders<SingleTrace>.Filter.Empty;
    }
}