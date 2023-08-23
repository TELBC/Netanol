using System.Data;
using Fennec.Controllers;
using Fennec.Database;
using Fennec.Database.Domain.Layout;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fennec.Tests.Controllers;

public class LayoutControllerTests
{
    private readonly LayoutController _controller;
    private readonly Mock<ILayoutRepository> _layoutRepositoryMock;

    public LayoutControllerTests()
    {
        _layoutRepositoryMock = new Mock<ILayoutRepository>();
        _controller = new LayoutController(_layoutRepositoryMock.Object);
    }

    [Fact]
    public async void List_ReturnsAllLayouts()
    {
        var layouts = new List<Layout>
        {
            new("Preset 1"),
            new("Preset 2")
        };

        _layoutRepositoryMock
            .Setup(repo => repo.ListLayouts())
            .ReturnsAsync(layouts);

        var result = await _controller.List();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(layouts, okResult.Value);
    }

    [Fact]
    public async void Create_ReturnsCreatedLayout()
    {
        var layout = new Layout("New Preset");

        _layoutRepositoryMock
            .Setup(repo => repo.CreateLayout(It.IsAny<string>()))
            .ReturnsAsync(layout);

        var result = await _controller.Create("New Preset");

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(layout, createdAtActionResult.Value);
    }

    [Fact]
    public async void Create_ReturnsBadRequest_WhenDuplicateName()
    {
        _layoutRepositoryMock
            .Setup(repo => repo.CreateLayout(It.IsAny<string>()))
            .ThrowsAsync(new DuplicateNameException("A layout with the name New Preset already exists."));

        var result = await _controller.Create("New Preset");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async void Rename_ReturnsUpdatedLayout()
    {
        var layout = new Layout("Updated Preset");

        _layoutRepositoryMock
            .Setup(repo => repo.RenameLayout(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(layout);

        var result = await _controller.Rename("Old Preset", "Updated Preset");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(layout, okResult.Value);
    }

    [Fact]
    public async void Rename_ReturnsNotFound_WhenLayoutNotFound()
    {
        _layoutRepositoryMock
            .Setup(repo => repo.RenameLayout(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("A layout with the name Old Preset does not exist."));

        var result = await _controller.Rename("Old Preset", "Updated Preset");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async void Delete_ReturnsDeletedLayout()
    {
        var layout = new Layout("Preset to delete");

        _layoutRepositoryMock
            .Setup(repo => repo.DeleteLayout(It.IsAny<string>()))
            .ReturnsAsync(layout);

        var result = await _controller.Delete("Preset to delete");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(layout, okResult.Value);
    }

    [Fact]
    public async void Delete_ReturnsNotFound_WhenLayoutNotFound()
    {
        _layoutRepositoryMock
            .Setup(repo => repo.DeleteLayout(It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("A layout with the name Preset to delete does not exist."));

        var result = await _controller.Delete("Preset to delete");

        Assert.IsType<NotFoundObjectResult>(result);
    }
}