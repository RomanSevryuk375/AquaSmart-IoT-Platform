using BuildingBlocks.Domain.Abstractions;

namespace Device.Application.Features.Mqtt;

public sealed record ProcessCommandAckCommand : ICommand
{
    public string TopicMac { get; init; } = string.Empty;
    public Guid CommandId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public DateTime? ExecutedAt { get; init; }
}
