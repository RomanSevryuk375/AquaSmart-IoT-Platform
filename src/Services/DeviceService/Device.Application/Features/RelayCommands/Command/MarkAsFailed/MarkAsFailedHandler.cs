using Device.Application.DTOs.RelayCommands;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.MarkAsFailed;

internal sealed class MarkAsFailedHandler(
    IRelayCommandsRepository queueRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkAsFailedCommand, Result>
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
                    $"{nameof(RelayCommand)} {request.CommandId} not found"));
        }

        Result ownership = await securityService.EnsureDeviceAccessAsync(
            command.ControllerId, request.DeviceToken, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<IReadOnlyList<RelayCommandResponseDto>>.Failure(ownership.Error);
        }

        if (command.Status == CommandStatus.Failed)
        {
            return Result.Success();
        }

        command.MarkAsFailed(request.ErrorMessage);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
