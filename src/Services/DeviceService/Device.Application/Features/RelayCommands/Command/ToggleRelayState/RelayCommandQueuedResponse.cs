namespace Device.Application.Features.RelayCommands.Command.ToggleRelayState;

public sealed record RelayCommandQueuedResponse
{
    public Guid CommandId { get; init; }
    public Guid RelayId { get; init; }
    public bool TargetState { get; init; }
    public string Status { get; init; } = string.Empty;
}
