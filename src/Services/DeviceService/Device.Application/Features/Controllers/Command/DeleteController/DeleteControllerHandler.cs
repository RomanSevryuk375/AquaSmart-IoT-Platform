namespace Device.Application.Features.Controllers.Command.DeleteController;

internal sealed class DeleteControllerHandler(
    IControllerRepository controllerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteControllerCommand, Result>
{
    public async Task<Result> Handle(
        DeleteControllerCommand request,
        CancellationToken cancellationToken)
    {
        await controllerRepository.DeleteAsync(request.ControllerId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
