using Device.Application.Interfaces;

namespace Device.Application.Behaviors;

public sealed class CommandSecurityBehavior<TRequest, TResponse>(
    IRelayCommandsRepository commandsRepository,
    IDeviceSecurityService securityService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICommandBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        RelayCommand? command = await commandsRepository.GetByIdAsync(
            request.CommandId, cancellationToken);
        if (command is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<RelayCommand>(
                string.Format(ErrorMessages.RelayCommandNotFound, request.CommandId)));
        }


        Result ownership = await securityService.EnsureDeviceAccessAsync(
            command.ControllerId, request.DeviceToken, cancellationToken);
        if (ownership.IsFailure)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(ownership.Error);
        }

        return await next();
    }
}
