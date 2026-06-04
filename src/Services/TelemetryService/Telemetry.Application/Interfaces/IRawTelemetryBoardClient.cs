using Contracts.Enums;
using Telemetry.Application.DTOs;

namespace Telemetry.Application.Interfaces;

public interface IRawTelemetryBoardClient
{
    Task TelemetryRawReceived(TelemetryRawChartPointDto rawChartPoint);

    Task AggregatePointGenerated(PeriodTypeEnum period, TelemetryChartPointDto point);
}
