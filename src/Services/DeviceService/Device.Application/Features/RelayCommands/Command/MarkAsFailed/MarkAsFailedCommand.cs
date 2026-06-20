using Contracts.Abstractions;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.MarkAsFailed;

internal sealed record MarkAsFailedCommand : ICommand, ICommandBoundRequest
{
    public Guid CommandId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}
