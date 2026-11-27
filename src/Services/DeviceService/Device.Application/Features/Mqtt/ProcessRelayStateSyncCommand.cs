using BuildingBlocks.Domain.Abstractions;

namespace Device.Application.Features.Mqtt;

public sealed record MqttRelayStateItemDto
{
    public Guid RelayId { get; init; }
    public bool State { get; init; }
    public bool IsManual { get; init; }
}

public sealed record ProcessRelayStateSyncCommand : ICommand
{
    public string TopicMac { get; init; } = string.Empty;
    public string? PayloadMacAddress { get; init; }
    public string Source { get; init; } = string.Empty;
    public DateTime? RecordedAt { get; init; }
    public IReadOnlyList<MqttRelayStateItemDto> Relays { get; init; } = [];
}
