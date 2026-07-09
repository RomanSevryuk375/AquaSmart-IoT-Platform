using Contracts.Enums;
using Telemetry.Application.DTOs;

namespace Telemetry.Application.Interfaces;

public interface IRawTelemetryBoardClient
{
    public Task TelemetryRawReceived(
        TelemetryRawChartPointDto rawChartPoint);

    public Task AggregatePointGenerated(
        PeriodType period,
        TelemetryChartPointDto point);
}
