using System.Diagnostics;
using System.Globalization;
using Fennec.Database.Domain;
using Fennec.Options;
using Fennec.Parsers;
using Fennec.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Fennec.Metrics;

public interface IApplicationStatus
{
    Dictionary<string, object?> GetLatestStatus();
}

public class ApplicationStatus : IApplicationStatus
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    
    public ApplicationStatus(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public Dictionary<string, object?> GetLatestStatus()
    {
        var status = new Dictionary<string, object?>();
        
        SetRuntime(status);
        SetDatabaseStatus(status);
        SetFlowCounts(status);
        SetMultiplexer(status);
        SetConfig(status);
        
        return status;
    }

    private void SetRuntime(IDictionary<string, object?> status)
    {
        status.Add("RunTime", new Dictionary<string, object?>
        {
            {"Start Time", _serviceProvider.GetService<ITimeService>()?.StartTime.ToString(CultureInfo.InvariantCulture)},
            {"Uptime", (DateTime.UtcNow - _serviceProvider.GetService<ITimeService>()!.StartTime).ToString()},
        });
    }
    
    private void SetDatabaseStatus(IDictionary<string, object?> status)
    {
        bool reachable;
        string? latency = null;
        string? totalSingleTraceCount = null;
        string? totalDatabaseSize = null;

        try
        {
            var stopwatch = Stopwatch.StartNew();
            _serviceProvider.GetService<IMongoClient>()?.ListDatabaseNames();
            stopwatch.Stop();

            reachable = true;
            latency = stopwatch.ElapsedMilliseconds + "ms";
            totalSingleTraceCount = _serviceProvider.GetService<IMongoDatabase>()!.GetCollection<SingleTrace>("singleTraces").CountDocuments(trace => true).ToString();
            totalDatabaseSize =
                _serviceProvider.GetService<IMongoDatabase>()!.RunCommand<BsonDocument>(new BsonDocument { { "dbStats", 1 } })["dataSize"].ToInt64() /
                 (1024 * 1024) + "MB";
        }
        catch (MongoConnectionException)
        {
            reachable = false;
        }
        
        status.Add("Database", new Dictionary<string, object?>
        {
            {"Reachable", reachable},
            {"Latency", latency},
            {"Single Trace Count", totalSingleTraceCount},
            {"Total Database Size", totalDatabaseSize}
        });
    }

    private void SetFlowCounts(IDictionary<string, object?> status)
    {
        var totalEntriesSinceUptime = _serviceProvider.GetService<IMongoDatabase>()!.GetCollection<SingleTrace>("singleTraces").CountDocuments(trace => trace.Timestamp >= _serviceProvider.GetService<ITimeService>()!.StartTime).ToString();

        var totalCounts = new Dictionary<string, string>();
        
        foreach (var protocol in Enum.GetValues(typeof(FlowProtocol)))
        {
            totalCounts.Add(protocol.ToString()!, (_serviceProvider.GetService<IMongoDatabase>()!.GetCollection<SingleTrace>("singleTraces").CountDocuments(trace => trace.FlowProtocol == (FlowProtocol)protocol)).ToString());
        }
        
        status.Add("Counting", new Dictionary<string, object>()
        {
            {"Total entries since up time", totalEntriesSinceUptime},
            {"Total counts", totalCounts}
        });
    }

    private void SetMultiplexer(IDictionary<string, object?> status)
    {
        var multiplexerOptions = _configuration.GetSection("Multiplexers").Get<List<MultiplexerOptions>>();
        var multiplexers = new Dictionary<string, object?>();

        if (multiplexerOptions != null)
            foreach (var multiplexer in multiplexerOptions)
            {
                if (multiplexer.Name != null)
                {
                    var multiplexersInfo = new Dictionary<string, object>
                    {
                        { "Enabled", multiplexer.Enabled },
                        { "Name", multiplexer.Name },
                        { "Port", multiplexer.ListeningPort.ToString() },
                        { "Accepted protocols", string.Join(", ", multiplexer.Parsers) }
                    };
                    multiplexers.Add(multiplexer.ListeningPort.ToString(), multiplexersInfo);
                }
            }

        status.Add("Multiplexers", multiplexers);
    }

    private void SetConfig(IDictionary<string, object?> status)
    {
        var vmware = new Dictionary<string, object?>();
        var dns = new Dictionary<string, object?>();
        var duplicateFlag = new Dictionary<string, object?>();

        var tagsRequestOptions = _serviceProvider.GetService<IOptions<TagsRequestOptions>>()?.Value;
        var tagsCacheOptions =  _serviceProvider.GetService<IOptions<TagsCacheOptions>>()?.Value;
        var duplicateOptions = _serviceProvider.GetService<IOptions<DuplicateFlaggingOptions>>()?.Value;
        var dnsCacheOptions = _serviceProvider.GetService<IOptions<DnsCacheOptions>>()?.Value;
        var dnsService = _serviceProvider.GetService<IDnsResolverService>();
        var tagsCacheService = _serviceProvider.GetService<ITagsCacheService>();
        var timeService = _serviceProvider.GetRequiredService<ITimeService>();
       
        vmware.Add("Enabled", tagsRequestOptions is { Enabled: true });
        if (tagsRequestOptions is {  Enabled: true } && tagsCacheOptions is not null)
        {
            vmware.Add("Target Server", tagsRequestOptions!.VmWareRequest.VmWareTargetAddress);
            vmware.Add("Cached Tags Count", tagsCacheService?.CountOfTags);
            vmware.Add("Refresh Period", tagsCacheOptions!.RefreshPeriod.ToString());
            vmware.Add("Last Refresh", tagsCacheService!.LastRefresh.ToString());
            vmware.Add("Next Refresh", ((tagsCacheService.LastRefresh ?? timeService.StartTime) + tagsCacheOptions.RefreshPeriod).ToString());
        }
        
        dns.Add("Enabled", dnsCacheOptions is { Enabled: true });
        if (dnsCacheOptions is not null)
        {
            dns.Add("Cached Entries Count", dnsService.CachedEntriesCount.ToString());
            dns.Add("Cleanup Interval", dnsCacheOptions.CleanupInterval.ToString());
            dns.Add("Last Cleanup", dnsService.LastCleanup.ToString());
            dns.Add("Next Cleanup", (dnsService.LastCleanup + dnsCacheOptions.CleanupInterval).ToString());
        }

        if (duplicateOptions is not null)
        {
            duplicateFlag.Add("Claim Life Time", duplicateOptions.ClaimExpirationLifespan.ToString());
            duplicateFlag.Add("Refresh Period", duplicateOptions.CleanupInterval.ToString());
        }

        status.Add("Services", new Dictionary<string, object>()
        {
            {"VMWare Tagging", vmware},
            {"DNS Server", dns},
            {"Duplicate Flagging", duplicateFlag}
        });
    }
}