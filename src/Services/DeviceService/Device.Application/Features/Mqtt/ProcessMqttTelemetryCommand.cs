using BuildingBlocks.Domain.Abstractions;

namespace Device.Application.Features.Mqtt;

public sealed record MqttTelemetryItemDto
{
    public Guid SensorId { get; init; }
    public double Value { get; init; }
    public string ExternalMessageId { get; init; } = string.Empty;
    public DateTime RecordedAt { get; init; }
}

public sealed record ProcessMqttTelemetryCommand : ICommand
{
    public string TopicMac { get; init; } = string.Empty;
    public string? PayloadMacAddress { get; init; }
    public IReadOnlyList<MqttTelemetryItemDto> Items { get; init; } = [];
}
