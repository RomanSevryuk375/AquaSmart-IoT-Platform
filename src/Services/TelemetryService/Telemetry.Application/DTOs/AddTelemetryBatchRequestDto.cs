// Ignore Spelling: Dto

using Contracts.Events.TelemetryEvents;

namespace Telemetry.Application.DTOs;

public sealed record AddTelemetryBatchRequestDto
{
    public string MacAddress { get; init; } = string.Empty;
    public List<TelemetryBatchEventItem> Items { get; init; } = [];
}
