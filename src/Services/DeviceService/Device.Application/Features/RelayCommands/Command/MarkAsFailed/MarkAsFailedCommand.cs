using BuildingBlocks.Domain.Abstractions;

namespace Device.Application.Features.RelayCommands.Command.MarkAsFailed;

public sealed record MarkAsFailedCommand : ICommand
{
    public Guid CommandId { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
}
