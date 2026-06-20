using Device.Application.Factories;
using MassTransit;

namespace Device.Application.Features.RelayCommands.Command.SetRelayState;

internal sealed class SetRelayStateHandler(
    IRelayRepository relayRepository,
    IControllerRepository controllerRepository,
    IRelayCommandsRepository queueRepository
    IUnitOfWork unitOfWork) : IRequestHandler<SetRelayStateCommand, Result>
{
    public async Task<Result> Handle(
        SetRelayStateCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound<Relay>(
                $"Relay {request.RelayId} not found"));
        }

        Controller? existingController = await controllerRepository.GetByIdAsync(
            request.ControllerId, cancellationToken);
        if (existingController is null)
        {
            return Result.Failure(Error.NotFound<Controller>(
                $"Controller {request.ControllerId} not found"));
        }

        if (UnavalibleCommand(request, existingRelay))
        {
            return Result.Failure(Error.Conflict<RelayCommand>(
                "Command is unavalible or was expired."));
        }

        Result<RelayCommand> newCommand = RelayCommand.Create(
            id: NewId.NextGuid(),
            existingController.Id,
            existingRelay.Id,
            request.TargetState,
            request.ExpireAt);

        if (newCommand.IsFailure)
        {
            return Result.Failure(newCommand.Error);
        }

        try
        {
            await queueRepository.AddAsync(newCommand.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure(
                "External.Error", ex.Message));
        }
    }

    private static bool UnavalibleCommand(
        SetRelayStateCommand request,
        Relay existingRelay)
    {
        return existingRelay.IsManual ||
              (request.ExpireAt.HasValue && request.ExpireAt.Value < DateTime.UtcNow) ||
               existingRelay.IsActive == StateEvaluatorFactory.EvaluateEnum(request.Action);
    }
}
