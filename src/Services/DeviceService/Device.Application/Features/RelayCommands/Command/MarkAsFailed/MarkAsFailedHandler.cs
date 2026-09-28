using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.RelayCommands.Command.MarkAsFailed;

internal sealed class MarkAsFailedHandler(IRelayCommandsRepository queueRepository)
    : IRequestHandler<MarkAsFailedCommand, Result>
{
    public async Task<Result> Handle(
        MarkAsFailedCommand request,
        CancellationToken cancellationToken)
    {
        RelayCommand? command = await queueRepository.GetByIdAsync(
            request.CommandId, cancellationToken);
        if (command is null)
        {
            return Result.Failure(Error.NotFound<RelayCommand>(
                    string.Format(ErrorMessages.RelayCommandNotFound, request.CommandId)));
        }

        if (command.Status == CommandStatus.Failed)
        {
            return Result.Success();
        }

        command.MarkAsFailed(request.ErrorMessage);

        return Result.Success();
    }
}
