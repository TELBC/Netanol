using System.Data;
using Fennec.Controllers;
using Fennec.Database;
using Fennec.Database.Domain.Layout;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fennec.Tests.Controllers;

public class LayoutPresetControllerTests
{
    private readonly LayoutPresetController _controller;
    private readonly Mock<ILayoutPresetRepository> _layoutPresetRepositoryMock;

    public LayoutPresetControllerTests()
    {
        _layoutPresetRepositoryMock = new Mock<ILayoutPresetRepository>();
        _controller = new LayoutPresetController(_layoutPresetRepositoryMock.Object);
    }

    [Fact]
    public async void List_ReturnsAllLayoutPresets()
    {
        var layoutPresets = new List<LayoutPreset>
        {
            new("Preset 1"),
            new("Preset 2")
        };

        _layoutPresetRepositoryMock
            .Setup(repo => repo.ListLayoutPresets())
            .ReturnsAsync(layoutPresets);

        var result = await _controller.List();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(layoutPresets, okResult.Value);
    }

    [Fact]
    public async void Create_ReturnsCreatedLayoutPreset()
    {
        var layoutPreset = new LayoutPreset("New Preset");

        _layoutPresetRepositoryMock
            .Setup(repo => repo.CreateLayoutPreset(It.IsAny<string>()))
            .ReturnsAsync(layoutPreset);

        var result = await _controller.Create("New Preset");

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(layoutPreset, createdAtActionResult.Value);
    }

    [Fact]
    public async void Create_ReturnsBadRequest_WhenDuplicateName()
    {
        _layoutPresetRepositoryMock
            .Setup(repo => repo.CreateLayoutPreset(It.IsAny<string>()))
            .ThrowsAsync(new DuplicateNameException("A layout with the name New Preset already exists."));

        var result = await _controller.Create("New Preset");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async void Rename_ReturnsUpdatedLayoutPreset()
    {
        var layoutPreset = new LayoutPreset("Updated Preset");

        _layoutPresetRepositoryMock
            .Setup(repo => repo.RenameLayoutPreset(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(layoutPreset);

        var result = await _controller.Rename("Old Preset", "Updated Preset");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(layoutPreset, okResult.Value);
    }

    [Fact]
    public async void Rename_ReturnsNotFound_WhenLayoutPresetNotFound()
    {
        _layoutPresetRepositoryMock
            .Setup(repo => repo.RenameLayoutPreset(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("A layout with the name Old Preset does not exist."));

        var result = await _controller.Rename("Old Preset", "Updated Preset");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async void Delete_ReturnsDeletedLayoutPreset()
    {
        var layoutPreset = new LayoutPreset("Preset to delete");

        _layoutPresetRepositoryMock
            .Setup(repo => repo.DeleteLayoutPreset(It.IsAny<string>()))
            .ReturnsAsync(layoutPreset);

        var result = await _controller.Delete("Preset to delete");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(layoutPreset, okResult.Value);
    }

    [Fact]
    public async void Delete_ReturnsNotFound_WhenLayoutPresetNotFound()
    {
        _layoutPresetRepositoryMock
            .Setup(repo => repo.DeleteLayoutPreset(It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("A layout with the name Preset to delete does not exist."));

        var result = await _controller.Delete("Preset to delete");

        Assert.IsType<NotFoundObjectResult>(result);
    }
}