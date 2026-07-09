using Contracts.Enums;
using Telemetry.Application.DTOs;

namespace Telemetry.Application.Interfaces;

public interface ITelemetryNotifier
{
    public Task AggregatePointGenerated(
        string ecosystemId,
        PeriodType period,
        TelemetryChartPointDto point);

    public Task TelemetryRawReceived(
        string ecosystemId,
        TelemetryRawChartPointDto rawChartPoint);
}
