using System.Net;
using Fennec.Options;
using Microsoft.Extensions.Options;

namespace Fennec.Services;

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

    public Task RequestLatestTagAndIps();
}

public class TagsCacheService : ITagsCacheService
{
    private readonly ILogger _log;
    private readonly ITagsRequestService _tagsRequestService;
    private Dictionary<IPAddress, List<string>> _ipTagsDict;

    public TagsCacheService(ILogger log, ITagsRequestService tagsRequestService)
    {
        _tagsRequestService = tagsRequestService;
        _log = log;
        _ipTagsDict = new Dictionary<IPAddress, List<string>>();
    }

    public List<string>? GetTags(IPAddress ipAddress)
        => _ipTagsDict.TryGetValue(ipAddress, out var tags) ? tags : null;

    public async Task RequestLatestTagAndIps()
    {
        _ipTagsDict = await _tagsRequestService.GetLatestTagsAndIps();
        _log.Information("Updated Vmware tags cache... Cache now has {CacheSize} IP entries", _ipTagsDict.Count);
    }
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
            _log.Information("Updating Vmware tags cache");
            await _tagsCacheService.RequestLatestTagAndIps();
            await Task.Delay(_options.RefreshPeriod, stoppingToken);
        }
    }
}