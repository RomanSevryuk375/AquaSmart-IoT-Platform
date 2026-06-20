using Device.Application.Interfaces;

namespace Device.Application.Features.Controllers.Command.UpdateController;

internal sealed class UpdateControllerHandler(
    IControllerRepository controllerRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateControllerCommand, Result>
{
    public async Task<Result> Handle(
        UpdateControllerCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Controller? controller = await controllerRepository
            .GetByIdAsync(request.ControllerId, cancellationToken);

        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            request.ControllerId, cancellationToken);
        if (ownership.IsFailure || controller is null)
        {
            return Result<ControllerResponseDto>
                .Failure(ownership.Error);
        }

        Result? result = controller.Update(
            request.MacAddress,
            request.Name);

        if (result is not null)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
