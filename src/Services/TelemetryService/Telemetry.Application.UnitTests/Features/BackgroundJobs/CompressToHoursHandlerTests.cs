using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Telemetry.Application.Features.BackgroundJobs.Commands.CompressToHours;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.UnitTests.Features.BackgroundJobs;

public class CompressToHoursHandlerTests
{
    private readonly ITelemetryAggregateDataRepository _telemetryAggregateMock;
    private readonly ICompressorHelper _compressorHelperMock;
    private readonly CompressToHoursHandler _handler;

    public CompressToHoursHandlerTests()
    {
        _telemetryAggregateMock = Substitute.For<ITelemetryAggregateDataRepository>();
        _compressorHelperMock = Substitute.For<ICompressorHelper>();
        _handler = new CompressToHoursHandler(_telemetryAggregateMock, _compressorHelperMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoUnaggregatedWindowsFound_ReturnsSuccessAndDoesNothing()
    {
        // Arrange
        _telemetryAggregateMock.GetUnaggregatedWindowsAsync(
            PeriodType.Minute, "hour", Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new CompressToHoursCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _telemetryAggregateMock.DidNotReceive().GetSummaryForPeriodAsync(
            Arg.Any<PeriodType>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUnaggregatedWindowsFound_CompressesToHourlyAndMarksAggregated()
    {
        // Arrange
        var hourStart = new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc);
        _telemetryAggregateMock.GetUnaggregatedWindowsAsync(
            PeriodType.Minute, "hour", Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([hourStart]);

        var sensorId = Guid.NewGuid();
        TelemetrySummary summary = TelemetrySummary.Create(15.0, 20.0, 25.0, 60).Value;
        var summaryData = new Dictionary<Guid, TelemetrySummary> { { sensorId, summary } };

        _telemetryAggregateMock.GetSummaryForPeriodAsync(
            PeriodType.Minute, hourStart, hourStart.AddHours(1), Arg.Any<CancellationToken>())
            .Returns(summaryData);

        var command = new CompressToHoursCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _compressorHelperMock.Received(1).CreateAndSaveAggregatedTelemetryAsync(
            sensorId, summary, hourStart, PeriodType.Hourly, Arg.Any<CancellationToken>());

        await _telemetryAggregateMock.Received(1).MarkAsAggregatedAsync(
            Arg.Is<List<Guid>>(l => l.Contains(sensorId)),
            PeriodType.Minute,
            hourStart,
            hourStart.AddHours(1),
            Arg.Any<CancellationToken>());
    }
}
