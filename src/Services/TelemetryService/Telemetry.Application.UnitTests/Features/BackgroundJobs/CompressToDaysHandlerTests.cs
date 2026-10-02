using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Telemetry.Application.Features.BackgroundJobs.Commands.CompressToDays;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.UnitTests.Features.BackgroundJobs;

public class CompressToDaysHandlerTests
{
    private readonly ITelemetryAggregateDataRepository _telemetryAggregateMock;
    private readonly ICompressorHelper _compressorHelperMock;
    private readonly CompressToDaysHandler _handler;

    public CompressToDaysHandlerTests()
    {
        _telemetryAggregateMock = Substitute.For<ITelemetryAggregateDataRepository>();
        _compressorHelperMock = Substitute.For<ICompressorHelper>();
        _handler = new CompressToDaysHandler(_telemetryAggregateMock, _compressorHelperMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoUnaggregatedWindowsFound_ReturnsSuccessAndDoesNothing()
    {
        // Arrange
        _telemetryAggregateMock.GetUnaggregatedWindowsAsync(
            PeriodType.Hourly, "day", Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new CompressToDaysCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _telemetryAggregateMock.DidNotReceive().GetSummaryForPeriodAsync(
            Arg.Any<PeriodType>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUnaggregatedWindowsFound_CompressesToDailyAndMarksAggregated()
    {
        // Arrange
        var dayStart = new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc);
        _telemetryAggregateMock.GetUnaggregatedWindowsAsync(
            PeriodType.Hourly, "day", Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([dayStart]);

        var sensorId = Guid.NewGuid();
        TelemetrySummary summary = TelemetrySummary.Create(14.0, 19.5, 26.0, 24).Value;
        var summaryData = new Dictionary<Guid, TelemetrySummary> { { sensorId, summary } };

        _telemetryAggregateMock.GetSummaryForPeriodAsync(
            PeriodType.Hourly, dayStart, dayStart.AddDays(1), Arg.Any<CancellationToken>())
            .Returns(summaryData);

        var command = new CompressToDaysCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _compressorHelperMock.Received(1).CreateAndSaveAggregatedTelemetryAsync(
            sensorId, summary, dayStart, PeriodType.Daily, Arg.Any<CancellationToken>());

        await _telemetryAggregateMock.Received(1).MarkAsAggregatedAsync(
            Arg.Is<List<Guid>>(l => l.Contains(sensorId)),
            PeriodType.Hourly,
            dayStart,
            dayStart.AddDays(1),
            Arg.Any<CancellationToken>());
    }
}
