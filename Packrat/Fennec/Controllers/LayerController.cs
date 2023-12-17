using AutoMapper;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Database.Domain.Layers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fennec.Controllers;

/// <summary>
///     Provides direct manipulation capabilities to change the layers of a layout.
/// </summary>
[SwaggerTag("Manage Layers within Layouts")]
[Route("layout/{layoutName}/layers")]
public class LayerController : ControllerBase
{
    private readonly ILayerRepository _layers;
    private readonly ILayoutRepository _layouts;
    private readonly IMapper _mapper;

    public LayerController(ILayoutRepository layouts, ILayerRepository layers, IMapper mapper)
    {
        _layouts = layouts;
        _layers = layers;
        _mapper = mapper;
    }

    /// <summary>
    ///     Insert a new layer into the given layout.
    /// </summary>
    /// <remarks>
    ///     If no index is provided, the layer will be appended to the end of the layout.
    /// </remarks>
    /// <param name="layoutName">The name of the layout</param>
    /// <param name="index"></param>
    /// <param name="layerDto"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, "Layer successfully inserted", typeof(FullLayoutDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest,
        "Invalid data was provided, reference the message for more information.")]
    public async Task<IActionResult> InsertLayer(string layoutName, [FromQuery] int? index,
        [FromBody] ILayerDto layerDto)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        var layer = _mapper.Map<ILayer>(layerDto);
        try
        {
            await _layers.InsertLayer(layout, layer, index);
        }
        catch (IndexOutOfRangeException e)
        {
            return BadRequest(e.Message);
        }

        return Ok(_mapper.Map<FullLayoutDto>(layout));
    }

    /// <summary>
    ///     Move a layer within a layout between two indices.
    /// </summary>
    /// <param name="layoutName"></param>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    /// <returns></returns>
    [HttpPut("{oldIndex}/{newIndex}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layer successfully moved", typeof(ILayerDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest,
        "Invalid data was provided, reference the message for more information.")]
    public async Task<IActionResult> MoveLayer(string layoutName, int oldIndex, int newIndex)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        try
        {
            await _layers.MoveLayer(layout, oldIndex, newIndex);
        }
        catch (IndexOutOfRangeException e)
        {
            return BadRequest(e.Message);
        }

        return Ok(_mapper.Map<FullLayoutDto>(layout));
    }

    /// <summary>
    ///     Delete a layer from a layout.
    /// </summary>
    /// <param name="layoutName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    [HttpDelete("{index}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layer successfully deleted", typeof(ILayerDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest,
        "Invalid data was provided, reference the message for more information.")]
    public async Task<IActionResult> DeleteLayer(string layoutName, int index)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        try
        {
            await _layers.DeleteLayer(layout, index);
        }
        catch (IndexOutOfRangeException e)
        {
            return BadRequest(e.Message);
        }

        return Ok(_mapper.Map<FullLayoutDto>(layout));
    }

    /// <summary>
    ///     Replace a layer in a layout with a new one.
    /// </summary>
    /// <param name="layoutName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    [HttpPut("{index}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layer successfully updated", typeof(ILayerDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest,
        "Invalid data was provided, reference the message for more information.")]
    public async Task<IActionResult> UpdateLayer(string layoutName, int index, [FromBody] ILayerDto layerDto)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        var layer = _mapper.Map<ILayer>(layerDto);
        try
        {
            await _layers.UpdateLayer(layout, index, layer);
        }
        catch (IndexOutOfRangeException e)
        {
            return BadRequest(e.Message);
        }

        return Ok(_mapper.Map<FullLayoutDto>(layout));
    }

    /// <summary>
    ///     Get a layer from a layout.
    /// </summary>
    /// <param name="layoutName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    [HttpGet("{index}")]
    public async Task<IActionResult> GetLayer(string layoutName, int index)
    {
        var layout = await _layouts.GetLayout(layoutName);
        if (layout == null)
            return NotFound("The given layout could not be found.");

        ILayer layer;
        try
        {
            layer = _layers.GetLayer(layout, index);
        }
        catch (IndexOutOfRangeException e)
        {
            return BadRequest(e.Message);
        }

        return Ok(_mapper.Map<ILayerDto>(layer));
    }
}