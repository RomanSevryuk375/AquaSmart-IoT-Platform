using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.IntegrationEvents.Events.Telemetrys;

namespace Telemetry.Application.Features.Telemetry.Commands.IngestDeviceTelemetryBatch;

public sealed record IngestDeviceTelemetryBatchCommand : ICommand
{
    public Guid ControllerId { get; init; }
    public Guid UserId { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public IReadOnlyList<TelemetryBatchEventItem> Items { get; init; } = [];
}
