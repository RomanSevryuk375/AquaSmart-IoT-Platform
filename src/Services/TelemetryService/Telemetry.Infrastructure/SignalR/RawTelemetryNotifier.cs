using Contracts.Enums;
using Microsoft.AspNetCore.SignalR;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.SignalR;

public sealed class RawTelemetryNotifier(
    IHubContext<TelemetryHub, IRawTelemetryBoardClient> hubContext) : ITelemetryNotifier
{
    public async Task AggregatePointGenerated(
        string ecosystemId,
        PeriodTypeEnum period, 
        TelemetryChartPointDto point)
    {
        await hubContext.Clients
            .Group(ecosystemId)
            .AggregatePointGenerated(period, point);
    }

    public async Task TelemetryRawReceived(
        string ecosystemId,
        TelemetryRawChartPointDto rawChartPoint)
    {
        await hubContext.Clients
            .Group(ecosystemId)
            .TelemetryRawReceived(rawChartPoint);
    }
}
