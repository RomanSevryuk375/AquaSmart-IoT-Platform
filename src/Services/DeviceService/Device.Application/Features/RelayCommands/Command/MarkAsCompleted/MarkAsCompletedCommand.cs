using BuildingBlocks.Domain.Abstractions;

namespace Device.Application.Features.RelayCommands.Command.MarkAsCompleted;

public sealed record MarkAsCompletedCommand : ICommand
{
    public Guid CommandId { get; init; }
}
