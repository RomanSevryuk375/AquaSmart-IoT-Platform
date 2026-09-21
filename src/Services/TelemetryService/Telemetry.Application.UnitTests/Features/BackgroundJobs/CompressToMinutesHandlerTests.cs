using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Telemetry.Application.Features.BackgroundJobs.Commands.CompressToMinutes;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.UnitTests.Features.BackgroundJobs;

public class CompressToMinutesHandlerTests
{
    private readonly ITelemetryRawDataRepository _telemetryRawMock;
    private readonly ICompressorHelper _compressorHelperMock;
    private readonly CompressToMinutesHandler _handler;

    public CompressToMinutesHandlerTests()
    {
        _telemetryRawMock = Substitute.For<ITelemetryRawDataRepository>();
        _compressorHelperMock = Substitute.For<ICompressorHelper>();
        _handler = new CompressToMinutesHandler(_telemetryRawMock, _compressorHelperMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoUnaggregatedWindowsFound_ReturnsSuccessAndDoesNothing()
    {
        // Arrange
        _telemetryRawMock.GetUnaggregatedMinuteWindowsAsync(
            Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new CompressToMinutesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _telemetryRawMock.DidNotReceive().GetSummaryForPeriodAsync(
            Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());

        await _compressorHelperMock.DidNotReceive().CreateAndSaveAggregatedTelemetryAsync(
            Arg.Any<Guid>(), Arg.Any<TelemetrySummary>(), Arg.Any<DateTime>(),
            Arg.Any<PeriodType>(), Arg.Any<CancellationToken>());

        await _telemetryRawMock.DidNotReceive().MarkAsAggregatedAsync(
            Arg.Any<List<Guid>>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenRawDataFound_CompressesDataMarksAsAggregated()
    {
        // Arrange
        var windowStart = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);
        _telemetryRawMock.GetUnaggregatedMinuteWindowsAsync(
            Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([windowStart]);

        var sensorId1 = Guid.NewGuid();
        var sensorId2 = Guid.NewGuid();
        TelemetrySummary summary1 = TelemetrySummary.Create(10.0, 15.0, 20.0, 5).Value;
        TelemetrySummary summary2 = TelemetrySummary.Create(5.0, 8.5, 12.0, 8).Value;

        var summaryData = new Dictionary<Guid, TelemetrySummary>
        {
            { sensorId1, summary1 },
            { sensorId2, summary2 }
        };

        _telemetryRawMock.GetSummaryForPeriodAsync(
            windowStart, windowStart.AddMinutes(1), Arg.Any<CancellationToken>())
            .Returns(summaryData);

        var command = new CompressToMinutesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _compressorHelperMock.Received(1).CreateAndSaveAggregatedTelemetryAsync(
            sensorId1, summary1, windowStart, PeriodType.Minute, Arg.Any<CancellationToken>());

        await _compressorHelperMock.Received(1).CreateAndSaveAggregatedTelemetryAsync(
            sensorId2, summary2, windowStart, PeriodType.Minute, Arg.Any<CancellationToken>());

        await _telemetryRawMock.Received(1).MarkAsAggregatedAsync(
            Arg.Is<List<Guid>>(list => list.Contains(sensorId1) && list.Contains(sensorId2) && list.Count == 2),
            windowStart,
            windowStart.AddMinutes(1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenMultipleOfflineWindowsExist_CompressesAllSequentialWindows()
    {
        // Arrange
        var window1 = new DateTime(2026, 9, 21, 11, 55, 0, DateTimeKind.Utc);
        var window2 = new DateTime(2026, 9, 21, 11, 56, 0, DateTimeKind.Utc);
        var window3 = new DateTime(2026, 9, 21, 11, 57, 0, DateTimeKind.Utc);

        _telemetryRawMock.GetUnaggregatedMinuteWindowsAsync(
            Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([window1, window2, window3]);

        var sensorId = Guid.NewGuid();
        TelemetrySummary summary = TelemetrySummary.Create(22.0, 23.0, 24.0, 10).Value;

        var summaryData = new Dictionary<Guid, TelemetrySummary> { { sensorId, summary } };

        _telemetryRawMock.GetSummaryForPeriodAsync(
            Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(summaryData);

        var command = new CompressToMinutesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _compressorHelperMock.Received(3).CreateAndSaveAggregatedTelemetryAsync(
            sensorId, summary, Arg.Any<DateTime>(), PeriodType.Minute, Arg.Any<CancellationToken>());

        await _telemetryRawMock.Received(1).MarkAsAggregatedAsync(
            Arg.Is<List<Guid>>(list => list.Contains(sensorId)), window1, window1.AddMinutes(1), Arg.Any<CancellationToken>());
        await _telemetryRawMock.Received(1).MarkAsAggregatedAsync(
            Arg.Is<List<Guid>>(list => list.Contains(sensorId)), window2, window2.AddMinutes(1), Arg.Any<CancellationToken>());
        await _telemetryRawMock.Received(1).MarkAsAggregatedAsync(
            Arg.Is<List<Guid>>(list => list.Contains(sensorId)), window3, window3.AddMinutes(1), Arg.Any<CancellationToken>());
    }
}
