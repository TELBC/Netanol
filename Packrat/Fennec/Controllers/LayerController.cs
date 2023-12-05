using Fennec.Database;
using Fennec.Database.Domain.Layers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fennec.Controllers;

/// <summary>
///     Represents a simplified layout with its data replaced by a short description.
/// </summary>
/// <param name="Type"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record ShortLayerDto(LayerType Type, string Name, bool Enabled, string Description);

/// <summary>
///     Fully fledged layer with an arbitrary data object.
/// </summary>
/// <param name="Type"></param>
/// <param name="Name"></param>
/// <param name="Enabled"></param>
/// <param name="Data"></param>
public record FullLayerDto(LayerType Type, string Name, bool Enabled, object Data);

/// <summary>
///     Provides direct manipulation capabilities to change the layers of a layout.
/// </summary>
[SwaggerTag("Manage Layers within Layouts")]
[Route("layout/{layoutName}/layers")]
public class LayerController : ControllerBase
{
    private readonly ILayoutRepository _layouts;

    public LayerController(ILayoutRepository layouts)
    {
        _layouts = layouts;
    }

    /// <summary>
    ///     Insert a new layer into the given layout at the given index.
    ///     If no index is given, the layer will be appended to the end.
    /// </summary>
    /// <param name="layoutName">The name of the layout</param>
    /// <param name="index"></param>
    /// <param name="creationRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, "The layer was successfully inserted.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest,
        "Invalid data was provided, reference the message for more information.")]
    public async Task<IActionResult> InsertLayer(string layoutName, [FromQuery] int? index,
        [FromBody] FullLayerDto creationRequest)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        ILayoutLayer? layer = creationRequest.Type switch
        {
            LayerType.Filter => new FilterLayer(),
            _ => null
        };
        if (layer == null)
            return BadRequest("The given layer type is not supported.");

        if (index == null)
            layout.Layers.Add(layer);
        else if (index < 0 || index > layout.Layers.Count)
            return BadRequest("The given index is out of bounds.");
        else
            layout.Layers.Insert(index.Value, layer);

        return Ok(layout);
    }

    [HttpPut("{oldIndex}/{newIndex}")]
    public async Task<IActionResult> MoveLayer(string layoutName, int oldIndex, int newIndex)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        return Ok();
    }

    [HttpGet("{index}")]
    public async Task<IActionResult> DeleteLayer(string layoutName, int index)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        return Ok();
    }

    [HttpPut("{index}")]
    public async Task<IActionResult> UpdateLayer(string layoutName, int index)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        return Ok();
    }

    [HttpDelete("{index}")]
    public async Task<IActionResult> GetLayer(string layoutName, int index)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        return Ok();
    }
}