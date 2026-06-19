using Device.Application.Interfaces;

namespace Device.Application.Services;

public sealed class DeviceSecurityService(
    IControllerRepository controllerRepository,
    IUserContext userContext,
    IMyHasher myHasher) : IDeviceSecurityService
{
    public async Task<Result> EnsureUserOwnsControllerAsync(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository.GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound(
                "Controller.NotFound",
                "Controller not found"));
        }

        if (controller.UserId != userContext.UserId)
        {
            return Result.Failure(Error.Conflict(
                "Access.Denied",
                "Forbidden: You don't own this controller"));
        }

        return Result.Success();
    }

    public async Task<Result> EnsureDeviceAccessAsync(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        var controller = await controllerRepository.GetByIdAsync(controllerId, cancellationToken);

        if (controller is null)
        {
            return Result.Failure(Error.NotFound(
                "Controller.NotFound",
                "Controller not found"));
        }

        if (!myHasher.Verify(deviceToken, controller.DeviceTokenHash))
        {
            return Result.Failure(Error.Conflict(
                "Access.Denied",
                "Invalid device token"));
        }

        return Result.Success();
    }
}
