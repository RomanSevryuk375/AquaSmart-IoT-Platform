using Device.Application.Interfaces;

namespace Device.Application.Features.Controllers.Command.DeleteController;

internal sealed class DeleteControllerHandler(
    IDeviceSecurityService securityService,
    IControllerRepository controllerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteControllerCommand, Result>
{
    public async Task<Result> Handle(
        DeleteControllerCommand request,
        CancellationToken cancellationToken)
    {
        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            request.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<ControllerResponseDto>.Failure(
                ownership.Error);
        }

        await controllerRepository.DeleteAsync(request.ControllerId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
