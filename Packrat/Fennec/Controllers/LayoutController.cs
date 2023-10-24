using System.Data;
using Fennec.Database;
using Fennec.Database.Domain.Layout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fennec.Controllers;

/// <summary>
///     Create, update and delete <see cref="Layout" />s.
/// </summary>
[Authorize]
[Route("layout")]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Manage Layouts")]
public class LayoutController : ControllerBase
{
    private readonly ILayoutRepository _layoutRepository;

    public LayoutController(ILayoutRepository layoutRepository)
    {
        _layoutRepository = layoutRepository;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "List all layouts, ordered by name")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layouts listed successfully")]
    public async Task<IActionResult> List()
    {
        var layouts = await _layoutRepository.ListLayouts();
        return Ok(layouts);
    }

    [HttpPost("{name}")]
    [SwaggerOperation(Summary = "Create a new layout")]
    [SwaggerResponse(StatusCodes.Status201Created, "Layout created successfully")]
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

    [HttpPut("{name}")]
    [SwaggerOperation(Summary = "Rename an existing layout")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layout renamed successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Layout not found")]
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

    [HttpDelete("{name}")]
    [SwaggerOperation(Summary = "Delete a layout")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layout deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Layout not found")]
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