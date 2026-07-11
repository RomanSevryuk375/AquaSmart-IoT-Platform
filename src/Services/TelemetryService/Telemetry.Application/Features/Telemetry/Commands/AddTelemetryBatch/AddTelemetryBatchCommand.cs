using Contracts.Abstractions;
using Contracts.Events.TelemetryEvents;

namespace Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;

public sealed record AddTelemetryBatchCommand : ICommand
{
    public string MacAddress { get; init; } = string.Empty;
    public string DeviceToken { get; init; } = string.Empty;
    public IReadOnlyList<TelemetryBatchEventItem> Items { get; init; } = [];
}
