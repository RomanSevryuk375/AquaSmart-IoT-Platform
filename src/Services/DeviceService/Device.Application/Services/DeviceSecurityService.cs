using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Device.Application.Interfaces;

namespace Device.Application.Services;

public sealed class DeviceSecurityService(
    IControllerRepository controllerRepository,
    IMyHasher myHasher) : IDeviceSecurityService
{
    public async Task<Result> EnsureUserOwnsControllerAsync(
        Guid controllerId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        Controller? controller = await controllerRepository.GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound<Controller>(
                ErrorMessages.ControllerNotFoundPlain));
        }

        if (controller.UserId != userId)
        {
            return Result.Failure(Error.Forbidden(
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
            return Result.Failure(Error.Unauthorized(
                ErrorMessages.AccessDenied,
                ErrorMessages.InvalidDeviceToken));
        }

        return Result.Success();
    }
}
