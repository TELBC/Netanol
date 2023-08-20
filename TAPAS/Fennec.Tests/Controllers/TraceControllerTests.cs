using System.Net;
using Fennec.Controllers;
using Fennec.Database;
using Fennec.Database.Domain.Technical;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Tests.Controllers;

public class TraceControllerTests
{
    private readonly TapasContext _context;
    private readonly TraceController _controller;

    public TraceControllerTests()
    {
        var options = new DbContextOptionsBuilder<TapasContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TapasContext(options);
        _controller = new TraceController(_context);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        // Seed NetworkHosts
        var host1 = new NetworkHost(IPAddress.Parse("192.168.0.1"));
        var host2 = new NetworkHost(IPAddress.Parse("192.168.0.2"));

        _context.NetworkHosts.AddRange(host1, host2);
        _context.SaveChanges();

        // Seed SingleTraces
        var trace = new SingleTrace(
            IPAddress.Parse("10.0.0.1"),
            DateTimeOffset.Now,
            TraceProtocol.Udp,
            host1,
            5000,
            host2,
            5001,
            100,
            10
        );

        _context.SingleTraces.Add(trace);
        _context.SaveChanges();
    }


    [Fact]
    public async Task GetTracesByWindow_ReturnsCorrectData()
    {
        // Act
        var result =
            await _controller.GetTracesByWindow(DateTimeOffset.Now.AddHours(-1), DateTimeOffset.Now.AddHours(1));

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var graph = Assert.IsType<GraphDto>(okResult.Value);

        Assert.True(graph.Nodes.Count > 0);
        Assert.True(graph.Edges.Count > 0);
        Assert.True(graph.Edges.First().Count > 0);
    }

    [Fact]
    public async Task GetTracesByWindow_ReturnsEmptyData_WhenOutsideWindow()
    {
        // Act
        var result =
            await _controller.GetTracesByWindow(DateTimeOffset.Now.AddHours(-5), DateTimeOffset.Now.AddHours(-3));

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var graph = Assert.IsType<GraphDto>(okResult.Value);

        Assert.Empty(graph.Nodes);
        Assert.Empty(graph.Edges);
    }
}