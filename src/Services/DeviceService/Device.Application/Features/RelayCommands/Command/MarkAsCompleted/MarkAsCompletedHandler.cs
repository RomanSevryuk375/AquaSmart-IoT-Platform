using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.RelayCommands.Command.MarkAsCompleted;

internal sealed class MarkAsCompletedHandler(
    IRelayCommandsRepository queueRepository,
    IRelayRepository relayRepository) : IRequestHandler<MarkAsCompletedCommand, Result>
{
    public async Task<Result> Handle(
        MarkAsCompletedCommand request,
        CancellationToken cancellationToken)
    {
        RelayCommand? command = await queueRepository.GetByIdAsync(
            request.CommandId, cancellationToken);
        if (command is null)
        {
            return Result.Failure(Error.NotFound<RelayCommand>(
                    string.Format(ErrorMessages.RelayCommandNotFound, request.CommandId)));
        }

        if (command.IsTerminal)
        {
            return Result.Success();
        }

        Relay? existingRelay = await relayRepository.GetByIdAsync(
            command.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound<Relay>(
                    string.Format(ErrorMessages.RelayNotFound, command.RelayId)));
        }

        existingRelay.SetState(command.TargetState, existingRelay.UserId);
        command.MarkAsCompleted();

        return Result.Success();
    }
}
