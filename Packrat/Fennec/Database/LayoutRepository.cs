using System.Data;
using Fennec.Database.Domain.Layout;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database;

public interface ILayoutRepository
{
    /// <summary>
    ///     Lists all layouts.
    /// </summary>
    /// <returns></returns>
    Task<List<Layout>> ListLayouts();

    /// <summary>
    ///     Creates a new layout with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="DuplicateNameException">Thrown if a layout with the name <paramref name="name" /> already exists.</exception>
    Task<Layout> CreateLayout(string name);

    /// <summary>
    ///     Renames a layout with the given name to the new name.
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if a layout with the name <paramref name="oldName" /> does not exist.</exception>
    /// <exception cref="DuplicateNameException">Thrown if a layout with the name <paramref name="newName" /> already exists.</exception>
    Task<Layout> RenameLayout(string oldName, string newName);

    /// <summary>
    ///     Deletes a layout with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if a layout with the name <paramref name="name" /> does not exist.</exception>
    Task<Layout> DeleteLayout(string name);
}

public class LayoutRepository : ILayoutRepository
{
    private readonly PackratContext _context;

    public LayoutRepository(PackratContext context)
    {
        _context = context;
    }

    public async Task<List<Layout>> ListLayouts()
    {
        // TODO: paging?
        // this should be fine for now, but if we have a lot of layouts this will be a problem
        return await _context.Layouts
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<Layout> CreateLayout(string name)
    {
        await ThrowIfLayoutExists(name);

        var layout = new Layout(name);
        _context.Layouts.Add(layout);
        await _context.SaveChangesAsync();
        return layout;
    }

    public async Task<Layout> RenameLayout(string oldName, string newName)
    {
        await ThrowIfLayoutExists(newName);

        var layout = await _context.Layouts.FirstOrDefaultAsync(l => l.Name == oldName);

        if (layout == null)
            throw new KeyNotFoundException($"A layout with the name {oldName} does not exist.");

        layout.Name = newName;
        await _context.SaveChangesAsync();
        return layout;
    }

    public async Task<Layout> DeleteLayout(string name)
    {
        var layout = await _context.Layouts.FirstOrDefaultAsync(l => l.Name == name);

        if (layout == null)
            throw new KeyNotFoundException($"A layout with the name {name} does not exist.");

        _context.Layouts.Remove(layout);
        await _context.SaveChangesAsync();
        return layout;
    }

    private async Task ThrowIfLayoutExists(string name)
    {
        var layout = await _context.Layouts.FirstOrDefaultAsync(l => l.Name == name);

        if (layout != null)
            throw new DuplicateNameException($"A layout with the name {name} already exists.");
    }
}