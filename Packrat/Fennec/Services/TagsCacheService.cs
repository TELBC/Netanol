using System.Net;
using Fennec.Options;
using Microsoft.Extensions.Options;

namespace Fennec.Services;

/// <summary>
/// Provides the tags for specified IPs.
/// </summary>
public interface ITagsCacheService
{
    /// <summary>
    ///     Returns tags if they exist for a specified <paramref name="ipAddress"/>
    /// </summary>
    /// <param name="ipAddress"></param>s
    /// <returns></returns>
    public List<string>? GetTags(IPAddress ipAddress);
    public Task RequestLatestTagAndIps();
}

public class TagsCacheService : ITagsCacheService 
{
    private Dictionary<IPAddress, List<string>> _ipTagsDict;
    private readonly PeriodicTimer _periodicTimer;
    private readonly ITagsRequestService _tagsRequestService;
    private readonly ILogger _logger;

    public TagsCacheService(ILogger logger, ITagsRequestService tagsRequestService)
    {
        _tagsRequestService = tagsRequestService;
        _logger = logger;
        _ipTagsDict = new Dictionary<IPAddress, List<string>>();
    }
    
    public List<string>? Test()
    {
        string ipAddressString = "10.20.0.1";
        IPAddress ipAddress = IPAddress.Parse(ipAddressString);
        return GetTags(ipAddress);
    }
    
    public List<string>? GetTags(IPAddress ipAddress)
    {
        return _ipTagsDict.TryGetValue(ipAddress, out var tags) ? tags : null;
    }

    public async Task RequestLatestTagAndIps()
    {
        _ipTagsDict = await _tagsRequestService.GetLatestTagsAndIps();
        _logger.Information("TagsCache has been updated");
    }
}

public class TagsCacheRefresherService : BackgroundService
{
    private readonly ITagsCacheService _tagsCacheService;
    private readonly ILogger _log;
    private readonly TimeSpan _refreshPeriod;

    public TagsCacheRefresherService(ITagsCacheService tagsCacheService, ILogger log,
        IOptions<TagsCacheOptions> options)
    {
        _tagsCacheService = tagsCacheService;
        _log = log;
        _refreshPeriod = TimeSpan.TryParse(options.Value.RefreshPeriod, out var result) ? result : TimeSpan.FromHours(24);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _log.Information("Running TagsCacheService.");
            await _tagsCacheService.RequestLatestTagAndIps();
            await Task.Delay(_refreshPeriod, stoppingToken);
        }
    }
}