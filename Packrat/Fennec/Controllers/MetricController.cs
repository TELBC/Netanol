using Fennec.Metrics;
using Fennec.Options;
using Fennec.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
    private readonly TimeSpan _flowMetricSavePeriod;

    public MetricController(IMetricService metricService, IFlowImporterMetric flowImporterMetric, IOptions<FlowImporterMetricsOptions> flowOptions)
    {
        _metricService = metricService;
        _metricFlowImporter = flowImporterMetric;
        _flowMetricSavePeriod = flowOptions.Value.FlowSavePeriod;
    }
    
    /// <summary>
    /// Gets all the flowImporter series data.
    /// </summary>
    [HttpGet("flowsSeries")]
    public async Task<IActionResult> GetFlowSeries(DateTime? from = null, DateTime? to = null)
    {
        from ??= DateTime.UtcNow.AddHours(-_flowMetricSavePeriod.TotalHours);
        to ??= DateTime.UtcNow;
        _metricFlowImporter.UpdateFlowSeriesMetric();

        var data = _metricService.GetMetrics<FlowSeriesData>("FlowSeriesData")
            .FlowImporterDataSeries;

        data.Where(fid => fid != null && fid.DateTime != null && fid.DateTime >= from && fid.DateTime <= to).ToArray();
        return Ok(data);
    }
    
    /// <summary>
    /// Gets all the flowImporter general data.
    /// </summary>
    [HttpGet("flowAggregated")]
    public async Task<IActionResult> GetFlowGeneral()
    {
        _metricFlowImporter.UpdateFlowGeneralMetric();
        var data = _metricService.GetMetrics<FlowGeneraData>("FlowGeneralData").EndPointsData;
        return Ok(data);
    }
}