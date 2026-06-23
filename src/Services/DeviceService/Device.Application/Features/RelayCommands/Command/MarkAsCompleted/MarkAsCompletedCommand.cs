using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.MarkAsCompleted;

public sealed record MarkAsCompletedCommand
    : ICommand, ICommandBoundRequest
{
    public Guid CommandId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
}
