using Device.Application.Interfaces;

namespace Device.Application.Features.Controllers.Command.ToggleCommandState;

internal sealed class ToggleControllerStateHandler(
    IControllerRepository controllerRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleControllerStateCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ToggleControllerStateCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Controller? controller = await controllerRepository
            .GetByIdAsync(request.ControllerId, cancellationToken);

        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            request.ControllerId, cancellationToken);
        if (ownership.IsFailure || controller is null)
        {
            return Result<bool>.Failure(
                ownership.Error);
        }

        controller.ToggleState();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(controller.IsOnline);
    }
}
