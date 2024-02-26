using Fennec.Metrics;
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
    private readonly IFlowImporterMetric _metricFlowImporter;

    public MetricController(IMetricService metricService, IFlowImporterMetric flowImporterMetric)
    {
        _metricService = metricService;
        _metricFlowImporter = flowImporterMetric;
    }
    
    /// <summary>
    /// Gets all the flowImporter data
    /// </summary>
    [HttpGet("flowImporter")]
    public async Task<IActionResult> GetFlowImporter()
    {
        _metricFlowImporter.UpdateMetric();
        var data = _metricService.GetMetrics<FlowImporterData>("FlowImporterData").FlowImporterDataSeries.ToArray();
        return Ok(data);
    }
}