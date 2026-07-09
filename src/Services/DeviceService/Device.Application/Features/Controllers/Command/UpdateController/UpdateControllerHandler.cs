namespace Device.Application.Features.Controllers.Command.UpdateController;

internal sealed class UpdateControllerHandler(
    IControllerRepository controllerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateControllerCommand, Result>
{
    public async Task<Result> Handle(
        UpdateControllerCommand request,
        CancellationToken cancellationToken)
    {
        Controller? controller = await controllerRepository.GetByIdAsync(
            request.ControllerId, cancellationToken);
        if (controller is null)
        {
            return Result<bool>.Failure(Error.NotFound<Controller>(
                string.Format(ErrorMessages.ControllerNotFound, request.ControllerId)));
        }

        Result? result = controller.Update(
            request.MacAddress,
            request.Name);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
