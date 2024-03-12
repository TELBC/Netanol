using Fennec.Metrics;
using Fennec.Options;
using Fennec.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
    private readonly IApplicationStatus _applicationStatus;

    public MetricController(IMetricService metricService, IFlowImporterMetric flowImporterMetric, IOptions<FlowImporterMetricsOptions> flowOptions, IApplicationStatus applicationStatus)
    {
        _metricService = metricService;
        _metricFlowImporter = flowImporterMetric;
        _flowMetricSavePeriod = flowOptions.Value.FlowSavePeriod;
        _applicationStatus = applicationStatus;
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
        
        return Ok(data.Where(fid => fid?.DateTime != null && fid.DateTime >= from && fid.DateTime <= to).ToArray()
        );
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

    [HttpGet("applicationStatus")]
    public async Task<IActionResult> GetApplicationStatus()
    {
        var data = _applicationStatus.GetLatestStatus();
        return Ok(data);
    }
}