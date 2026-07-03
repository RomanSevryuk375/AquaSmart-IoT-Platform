using Device.Application.Interfaces;

namespace Device.Application.Services;

public sealed class ControllerOfflineCheckerService(
    IControllerRepository repository,
    IUnitOfWork unitOfWork) : IControllerOfflineCheckerService
{
    public async Task<Result<int>> CheckAndDisableControllerAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            DateTime offlineThreshold = DateTime.UtcNow.AddMinutes(-5);

            IReadOnlyList<Controller> controllers = await repository.GetOfflineControllersAsync(
                offlineThreshold, cancellationToken);
            if (!controllers.Any())
            {
                return Result<int>.Success(0);
            }

            foreach (Controller controller in controllers)
            {
                controller.SetOffline();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(controllers.Count);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Failure(ErrorMessages.DatabaseError, ex.Message));
        }
    }
}
