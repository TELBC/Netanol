using System.Diagnostics;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Processing.Graph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fennec.Controllers;

public record GraphRequest(DateTimeOffset From, DateTimeOffset To, bool RemoveDisconnectedNodes = true);

public record GraphStatistics(long TotalHostCount, long TotalByteCount, long TotalPacketCount, long TotalTraceCount);

public record GraphResponse(
    GraphStatistics GraphStatistics,
    QueryConditions QueryConditions,
    List<TraceNodeDto> Nodes,
    List<TraceEdgeDto> Edges);

[Authorize]
[Route("graph/{layoutName}")]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Generate Graphs")]
public class GraphController : ControllerBase
{
    private readonly IGraphRepository _graphRepository;
    private readonly ILayoutRepository _layoutRepository;

    public GraphController(IGraphRepository graphRepository, ILayoutRepository layoutRepository)
    {
        _graphRepository = graphRepository;
        _layoutRepository = layoutRepository;
    }

    /// <summary>
    /// Generate the graph for a given layout within a specified timespan.
    /// </summary>
    /// <param name="layoutName">The name of the layout.</param>
    /// <param name="request">A JSON body containing the timespan for which the graph is requested.</param>
    /// <returns>An object containing the graph layout and its associated traces.</returns>
    /// <response code="200">Successfully returned the graph layout.</response>
    /// <response code="404">The layout specified by the name was not found.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateGraph(string layoutName, [FromBody] GraphRequest request)
    {
        var layout = await _layoutRepository.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        var stopwatch = Stopwatch.StartNew();
        var details = await _graphRepository.GenerateGraph(request, layout);
        stopwatch.Stop();
        
        // TODO: rework the return type
        var response = new GraphResponse(
                new GraphStatistics(
                    details.TotalHostCount,
                    details.TotalByteCount,
                    details.TotalPacketCount,
                    details.TotalTraceCount),
                layout.QueryConditions,
                details.Nodes,
                details.Edges); 
        
        return Ok(response);
    }
}
