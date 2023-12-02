using System.Data;
using Fennec.Database.Domain;
using MongoDB.Driver;

namespace Fennec.Database;

public interface ILayoutRepository
{
    /// <summary>
    /// Lists all layouts.
    /// </summary>
    /// <returns></returns>
    Task<List<Layout>> GetLayouts();

    /// <summary>
    /// Creates a new layout with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="DuplicateNameException">Thrown if a layout with the name <paramref name="name" /> already exists.</exception>
    Task<Layout> CreateLayout(string name);

    /// <summary>
    /// Renames a layout with the given name to the new name.
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if a layout with the <paramref name="oldName" /> does not exist.</exception>
    /// <exception cref="DuplicateNameException">Thrown if a layout with the <paramref name="newName" /> already exists.</exception>
    Task<Layout> RenameLayout(string oldName, string newName);

    /// <summary>
    ///  Deletes a layout with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if a layout with the name <paramref name="name" /> does not exist.</exception>
    Task<Layout> DeleteLayout(string name);
}

public class LayoutRepository : ILayoutRepository
{
    private readonly IMongoCollection<Layout> _layouts;

    public LayoutRepository(IMongoDatabase database)
    {
        _layouts = database.GetCollection<Layout>("layouts");
    }

    public async Task<List<Layout>> GetLayouts()
    {
        return await _layouts.Find(_ => true).ToListAsync();
    }

    public async Task<Layout> CreateLayout(string name)
    {
        var existingLayout = await _layouts.Find(l => l.Name == name).FirstOrDefaultAsync();
        if (existingLayout != null)
            throw new DuplicateNameException($"A layout with the name {name} already exists.");

        var newLayout = new Layout { Name = name };
        await _layouts.InsertOneAsync(newLayout);
        return newLayout;
    }

    public async Task<Layout> RenameLayout(string oldName, string newName)
    {
        var layout = await _layouts.Find(l => l.Name == oldName).FirstOrDefaultAsync();
        if (layout == null)
            throw new KeyNotFoundException($"A layout with the name {oldName} does not exist.");

        var existingLayoutWithNewName = await _layouts.Find(l => l.Name == newName).FirstOrDefaultAsync();
        if (existingLayoutWithNewName != null)
            throw new DuplicateNameException($"A layout with the name {newName} already exists.");

        var update = Builders<Layout>.Update.Set(l => l.Name, newName);
        await _layouts.UpdateOneAsync(l => l.Name == oldName, update);
        layout.Name = newName;
        return layout;
    }

    public async Task<Layout> DeleteLayout(string name)
    {
        var layout = await _layouts.FindOneAndDeleteAsync(l => l.Name == name);
        if (layout == null)
            throw new KeyNotFoundException($"A layout with the name {name} does not exist.");

        return layout;
    }
}
