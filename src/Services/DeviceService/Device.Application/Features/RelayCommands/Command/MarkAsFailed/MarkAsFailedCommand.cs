using Contracts.Abstractions;

namespace Device.Application.Features.RelayCommands.Command.MarkAsFailed;

internal sealed record MarkAsFailedCommand : ICommand
{
    public Guid CommandId { get; init; }
    public Guid ControllerId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}
