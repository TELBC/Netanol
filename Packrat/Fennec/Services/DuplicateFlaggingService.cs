using System.Collections.Concurrent;
using System.Net;
using Fennec.Database;
using Fennec.Options;
using Microsoft.Extensions.Options;

namespace Fennec.Services;

/// <summary>
/// Keeps track of which trace exporters are responsible for which traces.
/// </summary>
public interface IDuplicateFlaggingService
{
    /// <summary>
    /// Sets the flag of the given <see cref="TraceImportInfo"/> to whether it is a duplicate or not.
    /// </summary>
    /// <param name="importInfo"></param>
    public void FlagTrace(TraceImportInfo importInfo);
}

public class DuplicateFlaggingService : IDuplicateFlaggingService, IDisposable
{
    private readonly ITimeService _timeService;
    private readonly Timer _timer;
    private readonly DuplicateFlaggingOptions _options;
    private readonly ConcurrentDictionary<(IPAddress, IPAddress), (IPAddress, DateTime)> _originExporters = new();

    public DuplicateFlaggingService(ITimeService timeService, IOptions<DuplicateFlaggingOptions> options)
    {
        _options = options.Value;
        _timeService = timeService;
        _timer = new Timer(_ => EvictAllInvalid(), new object(), TimeSpan.Zero, _options.CleanupInterval);
    }

    public void FlagTrace(TraceImportInfo importInfo)
    {
        var key = (importInfo.SrcIp, importInfo.DstIp);
        EvictAndOrCreate(key, importInfo.ExporterIp);

        var value = _originExporters[key];
        
        // The exporter address is not the same as the current one thus we flag it as duplicate. 
        if (!importInfo.ExporterIp.Equals(value.Item1))
            importInfo.Duplicate = true;
    }

    /// <summary>
    /// Does nothing if the entry is still valid, otherwise remove it. Creates a new entry if one didn't exist or
    /// if the previous one was evicted.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="exporterAddress"></param>
    private void EvictAndOrCreate((IPAddress, IPAddress) key, IPAddress exporterAddress)
    {
        if (_originExporters.TryGetValue(key, out var value))
        {
            if (value.Item2 + _options.ClaimExpirationLifespan > _timeService.Now)
                return;
            
            _originExporters.TryRemove(key, out _);
        }

        _originExporters.TryAdd(key, (exporterAddress, _timeService.Now));
    }

    private void EvictAllInvalid()
    {
        var toRemove = _originExporters
            .Where(pair => pair.Value.Item2 + _options.ClaimExpirationLifespan >= _timeService.Now)
            .Select(pair => pair.Key)
            .ToList();
        
        foreach (var key in toRemove)
            _originExporters.TryRemove(key, out _);
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}