using Contracts.Events.ControllerEvents;
using Contracts.Results;
using Device.Application.Interfaces;
using Device.Domain.Interfaces;
using Device.Domain.Specifications;
using MassTransit;

namespace Device.Application.Services;

public sealed class ControllerOfflineCheckerService(
    IPublishEndpoint publishEndpoint,
    IControllerRepository repository,
    IUnitOfWork unitOfWork) : IControllerOfflineCheckerService
{
    public async Task<Result> CheckAndDisableController(CancellationToken cancellationToken)
    {
        try
        {
            var offlineThreshold = DateTime.UtcNow.AddMinutes(-5);
            var specification = new OfflineControllersSpecification(offlineThreshold);

            var controllers = await repository.GetAllAsync(
                specification,
                null,
                null,
                cancellationToken);

            if (!controllers.Any())
            {
                return Result.Success();
            }

            foreach (var controller in controllers)
            {
                controller.SetOffline();
                await repository.UpdateAsync(controller, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var controller in controllers)
            {
                await publishEndpoint.Publish(new ControllerNotOnlineEvent
                {
                    UserId = controller.UserId,
                    ControllerId = controller.Id,
                    LastSeenAt = controller.LastSeenAt,
                }, cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Job.DatabaseError", ex.Message));
        }
    }
}
