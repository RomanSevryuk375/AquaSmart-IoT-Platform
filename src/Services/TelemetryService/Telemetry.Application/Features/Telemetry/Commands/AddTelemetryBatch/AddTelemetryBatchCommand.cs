using Contracts.Abstractions;
using Contracts.Events.TelemetryEvents;

namespace Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;

public sealed record AddTelemetryBatchCommand : ICommand
{
    public Guid ControllerId { get; init; }
    public IReadOnlyList<TelemetryBatchEventItem> Items { get; init; } = [];
}
