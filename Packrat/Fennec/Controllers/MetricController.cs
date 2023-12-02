using Fennec.Collectors;
using Fennec.Database;
using Fennec.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fennec.Controllers;

/// <summary>
/// Provides the MetricService Data to frontend to show projects health.
/// </summary>
[ApiController]
[Authorize]
[Route("metrics")]
public class MetricController : ControllerBase
{
    private readonly IMetricService _metricService;
    private readonly IMetricRepository _metricRepository;
    private readonly IWriteLatencyCollector _writeLatencyCollector;

    public MetricController(IMetricService metricService, IMetricRepository metricRepository, IWriteLatencyCollector writeLatencyCollector)
    {
        _metricService = metricService;
        _metricRepository = metricRepository;
        _writeLatencyCollector = writeLatencyCollector;
    }
    
    /// <summary>
    /// Gets all metrics.
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllMetric()
    {
        _metricRepository.GetTotalCountAsync();
        _writeLatencyCollector.GetLatencies();
        return Ok(await _metricService.GetAllMetrics());
    }
}