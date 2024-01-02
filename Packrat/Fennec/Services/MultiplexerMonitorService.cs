using Fennec.Options;

namespace Fennec.Services;

public class MultiplexerMonitorService : BackgroundService
{
    private readonly ILogger _log;
    private readonly IEnumerable<MultiplexerService> _multiplexerServices;

    public MultiplexerMonitorService(IServiceProvider serviceProvider, IEnumerable<MultiplexerOptions> multiplexerOptions, ILogger log)
    {
        _log = log.ForContext<MultiplexerMonitorService>();
        _multiplexerServices = multiplexerOptions.Select(o => MultiplexerService.CreateInstance(serviceProvider, o)).ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        _log.Information("Registering {MultiplexerCount} multiplexers", _multiplexerServices.Count());
        var tasks = _multiplexerServices.Select(m => 
            Task.Run(async () => await m.RunAsync(token), default));

        _log.Debug("Monitor awaiting completion of all multiplexers");
        await Task.WhenAll(tasks);
    }
}