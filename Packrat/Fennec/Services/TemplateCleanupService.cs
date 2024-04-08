using System.Collections.Concurrent;
using System.Net;
using DotNetFlow.Ipfix;
using Fennec.Options;
using Microsoft.Extensions.Options;
using IpFixTemplateRecord = DotNetFlow.Ipfix.TemplateRecord;
using Netflow9TemplateRecord = DotNetFlow.Netflow9.TemplateRecord;

namespace Fennec.Services;

/// <summary>
/// Interface for cleanup service for IPFIX templates
/// </summary>
public interface IIpFixCleanupService
{
    Dictionary<(IPAddress, ushort), IpFixTemplateRecord> TemplateRecords { get; set; }
}

/// <summary>
/// Interface for cleanup service for NetFlow9 templates
/// </summary>
public interface INetFlow9CleanupService
{
    Dictionary<(IPAddress, ushort), Netflow9TemplateRecord> TemplateRecords { get; set; }
}

/// <summary>
/// Cleanup service for IPFIX templates
/// </summary>
public class IpFixTemplateCleanupService : BackgroundService, IIpFixCleanupService
{
    public Dictionary<(IPAddress, ushort), IpFixTemplateRecord> TemplateRecords { get; set; }
    private readonly TimeSpan _cleanupInterval;

    public IpFixTemplateCleanupService(IOptions<TemplateCleanupOptions> options)
    {
        TemplateRecords = new Dictionary<(IPAddress, ushort), IpFixTemplateRecord>();
        _cleanupInterval = options.Value.IpFixCleanupInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_cleanupInterval, stoppingToken);

            TemplateRecords.Clear();
        }
    }
}

/// <summary>
/// Cleanup service for NetFlow9 templates
/// </summary>
public class NetFlow9TemplateCleanupService : BackgroundService, INetFlow9CleanupService
{
    public Dictionary<(IPAddress, ushort), Netflow9TemplateRecord> TemplateRecords { get; set; }
    private readonly TimeSpan _cleanupInterval;


    public NetFlow9TemplateCleanupService(IOptions<TemplateCleanupOptions> options)
    {
        TemplateRecords = new Dictionary<(IPAddress, ushort), Netflow9TemplateRecord>();
        _cleanupInterval = options.Value.NetFlow9CleanupInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_cleanupInterval, stoppingToken);

            TemplateRecords.Clear();
        }
    }
}