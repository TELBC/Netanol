using System.Data;
using Fennec.Database.Domain.Layout;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database;

public interface ILayoutPresetRepository
{
    /// <summary>
    ///     Lists all layout presets.
    /// </summary>
    /// <returns></returns>
    Task<List<LayoutPreset>> ListLayoutPresets();

    /// <summary>
    ///     Creates a new layout with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="DuplicateNameException">Thrown if a layout with the name <paramref name="name" /> already exists.</exception>
    Task<LayoutPreset> CreateLayoutPreset(string name);

    /// <summary>
    ///     Renames a layout with the given name to the new name.
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if a layout with the name <paramref name="oldName" /> does not exist.</exception>
    /// <exception cref="DuplicateNameException">Thrown if a layout with the name <paramref name="newName" /> already exists.</exception>
    Task<LayoutPreset> RenameLayoutPreset(string oldName, string newName);

    /// <summary>
    ///     Deletes a layout with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if a layout with the name <paramref name="name" /> does not exist.</exception>
    Task<LayoutPreset> DeleteLayoutPreset(string name);
}

public class LayoutPresetRepository : ILayoutPresetRepository
{
    private readonly TapasContext _context;

    public LayoutPresetRepository(TapasContext context)
    {
        _context = context;
    }

    public async Task<List<LayoutPreset>> ListLayoutPresets()
    {
        // TODO: paging?
        // this should be fine for now, but if we have a lot of layouts this will be a problem
        return await _context.LayoutPresets
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<LayoutPreset> CreateLayoutPreset(string name)
    {
        await ThrowIfLayoutExists(name);

        var layoutPreset = new LayoutPreset(name);
        _context.LayoutPresets.Add(layoutPreset);
        await _context.SaveChangesAsync();
        return layoutPreset;
    }

    public async Task<LayoutPreset> RenameLayoutPreset(string oldName, string newName)
    {
        await ThrowIfLayoutExists(newName);

        var layoutPreset = await _context.LayoutPresets.FirstOrDefaultAsync(l => l.Name == oldName);

        if (layoutPreset == null)
            throw new KeyNotFoundException($"A layout with the name {oldName} does not exist.");

        layoutPreset.Name = newName;
        await _context.SaveChangesAsync();
        return layoutPreset;
    }

    public async Task<LayoutPreset> DeleteLayoutPreset(string name)
    {
        var layoutPreset = await _context.LayoutPresets.FirstOrDefaultAsync(l => l.Name == name);

        if (layoutPreset == null)
            throw new KeyNotFoundException($"A layout with the name {name} does not exist.");

        _context.LayoutPresets.Remove(layoutPreset);
        await _context.SaveChangesAsync();
        return layoutPreset;
    }

    private async Task ThrowIfLayoutExists(string name)
    {
        var layoutPreset = await _context.LayoutPresets.FirstOrDefaultAsync(l => l.Name == name);

        if (layoutPreset != null)
            throw new DuplicateNameException($"A layout with the name {name} does not exist.");
    }
}