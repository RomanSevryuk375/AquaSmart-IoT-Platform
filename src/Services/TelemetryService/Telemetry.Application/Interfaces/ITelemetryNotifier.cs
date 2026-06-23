using Contracts.Enums;
using Telemetry.Application.DTOs;

namespace Telemetry.Application.Interfaces;

public interface ITelemetryNotifier
{
    Task AggregatePointGenerated(
        string ecosystemId,
        PeriodType period,
        TelemetryChartPointDto point);

    Task TelemetryRawReceived(
        string ecosystemId, 
        TelemetryRawChartPointDto rawChartPoint);
}
