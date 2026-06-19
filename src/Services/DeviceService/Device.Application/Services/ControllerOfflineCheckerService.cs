using Device.Application.Interfaces;
using Device.Domain.Specifications;

namespace Device.Application.Services;

public sealed class ControllerOfflineCheckerService(
    IControllerRepository repository,
    IUnitOfWork unitOfWork) : IControllerOfflineCheckerService
{
    public async Task<Result> CheckAndDisableControllerAsync(CancellationToken cancellationToken)
    {
        try
        {
            DateTime offlineThreshold = DateTime.UtcNow.AddMinutes(-5);
            var specification = new OfflineControllersSpecification(offlineThreshold);

            IReadOnlyList<Controller> controllers = await repository.GetAllAsync(
                specification,
                cancellationToken);

            if (!controllers.Any())
            {
                return Result.Success();
            }

            foreach (Controller controller in controllers)
            {
                controller.SetOffline();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Job.DatabaseError", ex.Message));
        }
    }
}
