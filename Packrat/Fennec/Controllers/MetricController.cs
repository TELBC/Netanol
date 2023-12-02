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
    private readonly IWriteLatencyMetric _writeLatencyMetric;

    public MetricController(IMetricService metricService, IMetricRepository metricRepository, IWriteLatencyMetric writeLatencyMetric)
    {
        _metricService = metricService;
        _metricRepository = metricRepository;
        _writeLatencyMetric = writeLatencyMetric;
    }

    [HttpGet("writeLatencyMetricsForStats")]
    public OkObjectResult GetPeriodWriteLatencyMetrics()
    {
        _writeLatencyMetric.UpdateWriteLatencyMetricsForStats();
        return Ok(_metricService.GetMetrics<WriteLatencyMetricsForStats>("WriteLatencyMetricsForStats"));
    }
}