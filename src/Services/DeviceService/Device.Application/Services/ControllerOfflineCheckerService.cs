using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Device.Application.Services;

public sealed class ControllerOfflineCheckerService(
    IControllerRepository repository,
    IUnitOfWork unitOfWork,
    IOptions<DeviceSettings>? deviceOptions = null) : IControllerOfflineCheckerService
{
    private readonly DeviceSettings _settings = deviceOptions?.Value ?? new DeviceSettings();

    public async Task<Result<int>> CheckAndDisableControllerAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            DateTime now = DateTime.UtcNow;
            int graceSeconds = Math.Max(1, _settings.OfflineAlertGraceSeconds);
            DateTime graceThreshold = now.AddSeconds(-graceSeconds);
            DateTime fallbackThreshold = now.AddMinutes(-5);

            IReadOnlyList<Controller> offlinePendingAlert = await repository.GetOfflinePendingAlertAsync(
                graceThreshold, cancellationToken);
            foreach (Controller controller in offlinePendingAlert)
            {
                controller.MarkOfflineAlertSent();
            }

            IReadOnlyList<Controller> silentOnlineControllers = await repository.GetOfflineControllersAsync(
                fallbackThreshold, cancellationToken);
            foreach (Controller controller in silentOnlineControllers)
            {
                controller.SetOffline();
            }

            int totalAffected = offlinePendingAlert.Count + silentOnlineControllers.Count;
            if (totalAffected == 0)
            {
                return Result<int>.Success(0);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(totalAffected);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Failure(ErrorMessages.DatabaseError, ex.Message));
        }
    }
}
