using System.Data;
using Fennec.Database;
using Fennec.Database.Domain.Layout;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fennec.Controllers;

/// <summary>
///     Create, update and delete <see cref="Layout" />s.
/// </summary>
[Route("layout/{name}")]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Manage Layout Presets")]
public class LayoutController : ControllerBase
{
    private readonly ILayoutRepository _layoutRepository;

    public LayoutController(ILayoutRepository layoutRepository)
    {
        _layoutRepository = layoutRepository;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "List all layout presets, ordered by name")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layout presets listed successfully")]
    public async Task<IActionResult> List()
    {
        var layouts = await _layoutRepository.ListLayouts();
        return Ok(layouts);
    }

    [HttpPost("")]
    [SwaggerOperation(Summary = "Create a new layout preset")]
    [SwaggerResponse(StatusCodes.Status201Created, "Layout preset created successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A layout with the same name already exists")]
    public async Task<IActionResult> Create(string name)
    {
        try
        {
            var layout = await _layoutRepository.CreateLayout(name);
            return CreatedAtAction(nameof(Create), layout);
        }
        catch (DuplicateNameException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("")]
    [SwaggerOperation(Summary = "Rename an existing layout preset")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layout preset renamed successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Layout preset not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A layout with the new name already exists")]
    public async Task<IActionResult> Rename(string name, [FromQuery] string newName)
    {
        try
        {
            var layout = await _layoutRepository.RenameLayout(name, newName);
            return Ok(layout);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (DuplicateNameException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("")]
    [SwaggerOperation(Summary = "Delete a layout preset")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layout preset deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Layout preset not found")]
    public async Task<IActionResult> Delete(string name)
    {
        try
        {
            var layout = await _layoutRepository.DeleteLayout(name);
            return Ok(layout);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}