using System.Data;
using AutoMapper;
using Fennec.Database;
using Fennec.Database.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fennec.Controllers;

/// <summary>
/// Create, update and delete <see cref="Layout" />s.
/// </summary>
[Authorize]
[Route("layout")]
[ApiController]
[Produces("application/json")]
[SwaggerTag("Manage Layouts")]
public class LayoutController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILayoutRepository _layoutRepository;

    public LayoutController(ILayoutRepository layoutRepository, IMapper mapper)
    {
        _layoutRepository = layoutRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// List all layouts, ordered by name.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "All layouts successfully returned", typeof(List<ShortLayoutDto>))]
    public async Task<IActionResult> List()
    {
        var layouts = await _layoutRepository.GetLayouts();
        var dtos = layouts.Select(l => _mapper.Map<ShortLayoutDto>(l));
        return Ok(dtos);
    }

    /// <summary>
    /// Create a new layout with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost("{name}")]
    [SwaggerResponse(StatusCodes.Status201Created, "Layout successfully created", typeof(FullLayoutDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A layout with the name already exists")]
    public async Task<IActionResult> Create(string name)
    {
        try
        {
            var layout = await _layoutRepository.CreateLayout(name);
            var dto = _mapper.Map<FullLayoutDto>(layout);
            return CreatedAtAction(nameof(Create), dto);
        }
        catch (DuplicateNameException)
        {
            return BadRequest($"A layout with the same name `{name}` already exists.");
        }
    }

    /// <summary>
    /// Get a layout by name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpGet("{name}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layout successfully returned", typeof(Layout))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The layout with the name does not exist")]
    public async Task<IActionResult> Get(string name)
    {
        var layout = await _layoutRepository.GetLayout(name);
        if (layout == null)
            return NotFound($"The layout with the name `{name}` does not exist.");

        return Ok(_mapper.Map<FullLayoutDto>(layout));
    }

    /// <summary>
    /// Rename an existing layout
    /// </summary>
    /// <param name="name"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    [HttpPut("{name}/{newName}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layout successfully renamed", typeof(FullLayoutDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The layout with the name does not exist")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "A layout with the name already exists")]
    public async Task<IActionResult> Rename(string name, string newName)
    {
        try
        {
            var layout = await _layoutRepository.RenameLayout(name, newName);
            var dto = _mapper.Map<FullLayoutDto>(layout);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"The layout with the name `{name}` does not exist.");
        }
        catch (DuplicateNameException)
        {
            return BadRequest($"A layout with the name `{name}` already exists.");
        }
    }

    /// <summary>
    /// Delete a layout
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpDelete("{name}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Layout successfully deleted", typeof(FullLayoutDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The layout with the name does not exist")]
    public async Task<IActionResult> Delete(string name)
    {
        try
        {
            var layout = await _layoutRepository.DeleteLayout(name);
            return Ok(_mapper.Map<FullLayoutDto>(layout));
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"The layout with the name `{name}` does not exist.");
        }
    }
    
    /// <summary>
    /// Replace the existing query conditions for the layout with the given name with the new ones.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="queryConditions"></param>
    /// <returns></returns>
    [HttpPut("{name}/queryConditions")]
    [SwaggerResponse(StatusCodes.Status200OK, "Query conditions successfully updated", typeof(FullLayoutDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The layout with the name does not exist")]
    public async Task<IActionResult> ReplaceQueryConditions(string name, QueryConditionsDto queryConditions)
    {
        var layout = await _layoutRepository.GetLayout(name);
        if (layout == null)
            return NotFound($"The layout with the name `{name}` does not exist.");

        var newQueryConditions = _mapper.Map<QueryConditions>(queryConditions);
        await _layoutRepository.ReplaceQueryConditions(name, newQueryConditions);
        return Ok(_mapper.Map<FullLayoutDto>(layout));
    }
}
