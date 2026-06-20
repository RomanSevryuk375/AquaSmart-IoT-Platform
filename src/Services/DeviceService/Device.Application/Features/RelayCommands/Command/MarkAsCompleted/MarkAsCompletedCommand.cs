using Contracts.Abstractions;

namespace Device.Application.Features.RelayCommands.Command.MarkAsCompleted;

internal sealed record MarkAsCompletedCommand : ICommand
{
    public Guid CommandId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
}
