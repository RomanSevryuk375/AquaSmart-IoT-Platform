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
    public async Task Handle_WhenNoRawDataFound_ReturnsSuccessAndDoesNothing()
    {
        // Arrange
        _telemetryRawMock.GetSummaryForPeriodAsync(
            Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, TelemetrySummary>());

        var command = new CompressToMinutesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _compressorHelperMock.DidNotReceive().CreateAndSaveAggregatedTelemetryAsync(
            Arg.Any<Guid>(), Arg.Any<TelemetrySummary>(), Arg.Any<DateTime>(),
            Arg.Any<PeriodType>(), Arg.Any<CancellationToken>());

        await _telemetryRawMock.DidNotReceive().MarkAsAggregatedAsync(
            Arg.Any<List<Guid>>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());

        await _compressorHelperMock.DidNotReceive().NotifyClientsAsync(
            Arg.Any<IReadOnlyDictionary<Guid, TelemetrySummary>>(), Arg.Any<DateTime>(),
            Arg.Any<PeriodType>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenRawDataFound_CompressesDataMarksAsAggregatedAndNotifiesClients()
    {
        // Arrange
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
            Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(summaryData);

        var command = new CompressToMinutesCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _compressorHelperMock.Received(1).CreateAndSaveAggregatedTelemetryAsync(
            sensorId1, summary1, Arg.Any<DateTime>(), PeriodType.Minute, Arg.Any<CancellationToken>());

        await _compressorHelperMock.Received(1).CreateAndSaveAggregatedTelemetryAsync(
            sensorId2, summary2, Arg.Any<DateTime>(), PeriodType.Minute, Arg.Any<CancellationToken>());

        await _telemetryRawMock.Received(1).MarkAsAggregatedAsync(
            Arg.Is<List<Guid>>(list => list.Contains(sensorId1) && list.Contains(sensorId2) && list.Count == 2),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());

        await _compressorHelperMock.Received(1).NotifyClientsAsync(
            Arg.Is<IReadOnlyDictionary<Guid, TelemetrySummary>>(d =>
                d.ContainsKey(sensorId1) && d.ContainsKey(sensorId2)),
            Arg.Any<DateTime>(),
            PeriodType.Minute,
            Arg.Any<CancellationToken>());
    }
}
