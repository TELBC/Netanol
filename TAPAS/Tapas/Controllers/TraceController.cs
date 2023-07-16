using Microsoft.AspNetCore.Mvc;
using Tapas.Database;

namespace Tapas.Controllers;

[Route("trace")]
public class TraceController : ControllerBase
{
    private readonly TraceRepository _traceRepository;

    public TraceController(TraceRepository traceRepository)
    {
        _traceRepository = traceRepository;
    }

    [HttpGet("/get_all")]
    public async Task<IActionResult> GetAllTraces()
    {
        return Ok(_traceRepository.GetAllSingleTraces());
    }
}