using Fennec.Database.Domain;
using Fennec.Database.Domain.Layers;

namespace Fennec.Database;

/// <summary>
///     Handles layers within a <see cref="Layout" />.
/// </summary>
public interface ILayerRepository
{
    /// <summary>
    ///     Insert the <paramref name="layer" /> into the layout at the given index. Appends it to the end
    ///     if the index is not provided.
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="layer"></param>
    /// <param name="index"></param>
    /// <exception cref="IndexOutOfRangeException">
    ///     The given index is not within the range of <see cref="Layout.Layers" />
    /// </exception>
    /// <returns></returns>
    public Task<Layout> InsertLayer(Layout layout, ILayer layer, int? index = null);

    /// <summary>
    ///     Remove the <see cref="ILayer" /> at <paramref name="oldIndex" /> and insert it at <paramref name="newIndex" />.
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    /// <exception cref="IndexOutOfRangeException">
    ///     Either the <paramref name="oldIndex" /> or <paramref name="newIndex" />
    ///     are not within the bounds of the <see cref="Layout.Layers" />.
    /// </exception>
    /// <returns></returns>
    public Task<Layout> MoveLayer(Layout layout, int oldIndex, int newIndex);

    /// <summary>
    ///     Delete the layer at the given <paramref name="index" />.
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public Task<(Layout layout, ILayer layer)> DeleteLayer(Layout layout, int index);

    /// <summary>
    ///     Replace the <see cref="ILayer" /> at the <paramref name="index" /> with the given <paramref name="newLayer" />.
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="index"></param>
    /// <param name="newLayer"></param>
    /// <returns></returns>
    public Task<(Layout layout, ILayer oldLayer, ILayer newLayer)> UpdateLayer(Layout layout, int index,
        ILayer newLayer);

    /// <summary>
    ///     Get the layer at the given index.
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public ILayer GetLayer(Layout layout, int index);
}

public class LayerRepository : ILayerRepository
{
    private readonly ILayoutRepository _layouts;

    public LayerRepository(ILayoutRepository layouts)
    {
        _layouts = layouts;
    }

    public async Task<Layout> InsertLayer(Layout layout, ILayer layer, int? index = null)
    {
        index ??= layout.Layers.Count;

        if (index < 0 || index > layout.Layers.Count)
            throw new IndexOutOfRangeException("The given index is out of bounds.");

        layout.Layers.Insert(index.Value, layer);
        await _layouts.UpdateLayout(layout);
        return layout;
    }

    public Task<Layout> MoveLayer(Layout layout, int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= layout.Layers.Count)
            throw new IndexOutOfRangeException("The given old index is out of bounds.");

        if (newIndex < 0 || newIndex >= layout.Layers.Count)
            throw new IndexOutOfRangeException("The given new index is out of bounds.");

        var layer = layout.Layers[oldIndex];
        layout.Layers.RemoveAt(oldIndex);
        layout.Layers.Insert(newIndex, layer);
        return _layouts.UpdateLayout(layout);
    }

    public async Task<(Layout layout, ILayer layer)> DeleteLayer(Layout layout, int index)
    {
        if (index < 0 || index >= layout.Layers.Count)
            throw new IndexOutOfRangeException("The given index is out of bounds.");

        var layer = layout.Layers[index];
        layout.Layers.RemoveAt(index);
        return (await _layouts.UpdateLayout(layout), layer);
    }

    public async Task<(Layout layout, ILayer oldLayer, ILayer newLayer)> UpdateLayer(Layout layout, int index,
        ILayer newLayer)
    {
        if (index < 0 || index >= layout.Layers.Count)
            throw new IndexOutOfRangeException("The given index is out of bounds.");

        var oldLayer = layout.Layers[index];
        layout.Layers.RemoveAt(index);
        layout.Layers.Insert(index, newLayer);
        return (await _layouts.UpdateLayout(layout), oldLayer, newLayer);
    }

    public ILayer GetLayer(Layout layout, int index)
    {
        if (index < 0 || index >= layout.Layers.Count)
            throw new IndexOutOfRangeException("The given index is out of bounds.");
        return layout.Layers[index];
    }
}