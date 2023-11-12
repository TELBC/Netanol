using Fennec.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fennec.Controllers;

[ApiController]
//[Authorize]
[Route("metrics")]
public class MetricController : ControllerBase
{
    private readonly MetricService _metricService;

    public MetricController(MetricService metricService)
    {
        _metricService = metricService;
    }
   
    [HttpGet("all")]
    public async Task<IActionResult> GetAllMetric()
    {
        var (count, byteCountSum) = await _metricService.GetAllMetrics();
        return Ok(new 
        {
            count = count,
            byteCountSum = byteCountSum.GetValue("totalByteCount").AsInt64
        });
    }
}