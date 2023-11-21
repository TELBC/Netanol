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

    public MetricController(IMetricService metricService, IMetricRepository metricRepository)
    {
        _metricService = metricService;
        _metricRepository = metricRepository;
    }
    
    /// <summary>
    /// Gets all metrics.
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllMetric()
    {
        _metricRepository.GetTotalCountAsync();
        return Ok(await _metricService.GetAllMetrics());
    }
}