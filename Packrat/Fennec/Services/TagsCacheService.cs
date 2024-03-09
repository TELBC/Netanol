using System.Net;
using Fennec.Options;
using Fennec.Services;
using Microsoft.Extensions.Options;

namespace Fennec.Services;

// TODO: rename all TagsCacheServices to VmwareTaggingServices
/// <summary>
///     Provides the tags for specified IPs.
/// </summary>
public interface ITagsCacheService
{
    /// <summary>
    ///     Returns tags if they exist for a specified <paramref name="ipAddress" />
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public List<string>? GetTags(IPAddress ipAddress);

    public Task RefreshTags();
    
    public DateTimeOffset? LastRefresh { get; }
    
    public int CountOfTags { get; }
}

public class TagsCacheService : ITagsCacheService
{
    private readonly ILogger _log;
    private readonly ITagsRequestService _tagsRequestService;
    private Dictionary<IPAddress, List<string>> _ipTagsDict;
    public int CountOfTags => _ipTagsDict.Count;
    public DateTimeOffset? LastRefresh { get; private set; }

    public TagsCacheService(ILogger log, ITagsRequestService tagsRequestService)
    {
        _tagsRequestService = tagsRequestService;
        _log = log;
        _ipTagsDict = new Dictionary<IPAddress, List<string>>();
    }

    public List<string>? GetTags(IPAddress ipAddress)
        => _ipTagsDict.TryGetValue(ipAddress, out var tags) ? tags : null;

    public async Task RefreshTags()
    {
        _ipTagsDict = await _tagsRequestService.GetLatestTagsAndIps();
        LastRefresh = DateTimeOffset.Now;
        _log.Information("Updated VMware tags cache... Cache now has {CacheSize} IP entries", _ipTagsDict.Count);
    }
}

public class MockTagsCacheService : ITagsCacheService
{
    public int CountOfTags => 2;
    
    public List<string> GetTags(IPAddress ipAddress) => new() { "mock-tag-1/switch", "mock-tag-2/infra" };

    public Task RefreshTags() => Task.CompletedTask;
    public DateTimeOffset? LastRefresh => DateTimeOffset.Now;
}

public class TagsCacheRefresherService : BackgroundService
{
    private readonly ILogger _log;
    private readonly TagsCacheOptions _options;
    private readonly ITagsCacheService _tagsCacheService;
    
    public TagsCacheRefresherService(ITagsCacheService tagsCacheService, ILogger log,
        IOptions<TagsCacheOptions> options)
    {
        _tagsCacheService = tagsCacheService;
        _log = log;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _log.Information("Updating VMware tags cache");
            await _tagsCacheService.RefreshTags();
            await Task.Delay(_options.RefreshPeriod, stoppingToken);
        }
    }
}