using System.Net;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Options;
using Fennec.Services;
using NSubstitute;

namespace Fennec.Tests.Services;

public class DuplicateFlaggingTests
{
    private static readonly TraceImportInfo ImportInfo = new(
        DateTimeOffset.Now, IPAddress.Parse("192.168.0.1"),
        IPAddress.Parse("10.10.20.1"), 10,
        IPAddress.Parse("10.10.30.1"), 20,
        false,
        10, 10, TraceProtocol.Tcp);

    [Fact]
    public void NoFlag_WhenOnlyTrace()
    {
        // Arrange
        var timeService = Substitute.For<ITimeService>();
        timeService.Now.Returns(DateTime.Now);

        var options = Utils.GetOptions(new DuplicateFlaggingOptions
        {
            ClaimExpirationLifespan = TimeSpan.FromMinutes(1)
        });

        var importInfo = ImportInfo with { };
        var service = new DuplicateFlaggingService(timeService, options);

        // Act
        service.FlagTrace(importInfo);

        // Assert
        Assert.False(importInfo.Duplicate);
    }

    [Fact]
    public void Flag_WhenDuplicateExporter()
    {
        // Arrange
        var timeService = Substitute.For<ITimeService>();
        timeService.Now.Returns(DateTime.Now);

        var options = Utils.GetOptions(new DuplicateFlaggingOptions
        {
            ClaimExpirationLifespan = TimeSpan.FromMinutes(1)
        });

        var importInfoFromA = ImportInfo with { };
        var importInfoFromB = ImportInfo with { ExporterIp = IPAddress.Parse("20.20.20.20") };
        var service = new DuplicateFlaggingService(timeService, options);

        // Act
        service.FlagTrace(importInfoFromA);
        service.FlagTrace(importInfoFromB);
        service.FlagTrace(importInfoFromA);

        // Assert
        Assert.False(importInfoFromA.Duplicate);
        Assert.True(importInfoFromB.Duplicate);
    }

    [Fact]
    public void NoFlag_AfterDuplicateExporterEvicted()
    {
        // Arrange
        var timeService = Substitute.For<ITimeService>();
        timeService.Now.Returns(DateTime.Now);

        var options = Utils.GetOptions(new DuplicateFlaggingOptions
        {
            ClaimExpirationLifespan = TimeSpan.FromMinutes(1)
        });

        var fromA1 = ImportInfo with { };
        var fromA2 = ImportInfo with { };
        var fromB = ImportInfo with { ExporterIp = IPAddress.Parse("20.20.20.20") };
        var service = new DuplicateFlaggingService(timeService, options);

        // Act
        service.FlagTrace(fromA1);
        timeService.Now.Returns(DateTime.Now + TimeSpan.FromHours(1));
        service.FlagTrace(fromB);
        service.FlagTrace(fromA2);

        // Assert
        Assert.False(fromA1.Duplicate);
        Assert.False(fromB.Duplicate);
        Assert.True(fromA2.Duplicate);
    }

    [Fact]
    public async Task NoFlag_AfterTimerEviction()
    {
        // Arrange
        var timeService = Substitute.For<ITimeService>();
        timeService.Now.Returns(DateTime.Now);

        var options = Utils.GetOptions(new DuplicateFlaggingOptions
        {
            ClaimExpirationLifespan = TimeSpan.FromMinutes(1),
            CleanupInterval = TimeSpan.FromSeconds(5),
        });

        var fromA1 = ImportInfo with { };
        var fromA2 = ImportInfo with { };
        var fromB = ImportInfo with { ExporterIp = IPAddress.Parse("20.20.20.20") };
        var service = new DuplicateFlaggingService(timeService, options);

        // Act
        service.FlagTrace(fromB);
        service.FlagTrace(fromA1);
        await Task.Delay(TimeSpan.FromSeconds(7));
        service.FlagTrace(fromA2);

        // Assert
        Assert.False(fromB.Duplicate);
        Assert.True(fromA1.Duplicate);
        Assert.False(fromA2.Duplicate);
    }
}