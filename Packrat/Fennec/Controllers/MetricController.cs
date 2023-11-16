using Fennec.DTOs;
using Fennec.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fennec.Controllers;

[ApiController]
[Authorize]
[Route("metrics")]
public class MetricController : ControllerBase
{
    private readonly IMetricService _metricService;

    public MetricController(IMetricService metricService)
    {
        _metricService = metricService;
    }
   
    [HttpGet("all")]
    public async Task<MetricsDto> GetAllMetric()
    {
        var (countTotal, countLast12Hours, countLast24Hours, countLast72Hours) = await _metricService.GetAllMetrics();
        return new MetricsDto
        {
            CountTotal = countTotal,
            CountLast12Hours = countLast12Hours,
            CountLast24Hours = countLast24Hours,
            CountLast72Hours = countLast72Hours
        };
    }
}