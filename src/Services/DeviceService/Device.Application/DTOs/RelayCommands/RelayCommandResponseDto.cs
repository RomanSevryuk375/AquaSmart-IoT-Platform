using Contracts.Enums;

namespace Device.Application.DTOs.RelayCommands;

public record RelayCommandResponseDto
{
    public Guid Id { get; init; }
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public RuleActionEnum Action { get; init; }
    public CommandStatusEnum Status { get; init; }
    public DateTime? ExpireAt { get; init; }
    public int AttemptCount { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreatedAt { get; init; }
}
