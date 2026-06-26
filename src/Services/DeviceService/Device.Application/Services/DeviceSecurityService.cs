using Device.Application.Interfaces;

namespace Device.Application.Services;

public sealed class DeviceSecurityService(
    IControllerRepository controllerRepository,
    IUserContext userContext,
    IMyHasher myHasher) : IDeviceSecurityService
{
    public async Task<Result> EnsureUserOwnsControllerAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default)
    {
        Controller? controller = await controllerRepository.GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound<Controller>(
                ErrorMessages.ControllerNotFoundPlain));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                ErrorMessages.AccessDenied,
                ErrorMessages.YouDontOwnThisController));
        }

        return Result.Success();
    }

    public async Task<Result> EnsureDeviceAccessAsync(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken = default)
    {
        Controller? controller = await controllerRepository.GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound<Controller>(
                ErrorMessages.ControllerNotFoundPlain));
        }

        if (!myHasher.Verify(deviceToken, controller.DeviceTokenHash))
        {
            return Result.Failure(Error.Conflict(
                ErrorMessages.AccessDenied,
                ErrorMessages.InvalidDeviceToken));
        }

        return Result.Success();
    }
}
