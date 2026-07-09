using Contracts.Results;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.Specifications;

namespace Telemetry.Application.Services;

public class SensorStateCheckerService(
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork)
    : ISensorStateCheckerService
{
    private const int OfflineThreshold = -5;

    public async Task<Result> CheckStateAndNotify(CancellationToken cancellationToken)
    {
        DateTime offlineThreshold = DateTime.UtcNow.AddMinutes(OfflineThreshold);
        var specification = new SensorIsDelayedSpecification(offlineThreshold);

        var sensors = await sensorRepository.GetAllAsync(specification, null, null, cancellationToken);
        if (!sensors.Any())
        {
            return Result.Success();
        }

        foreach (var sensor in sensors)
        {
            sensor.MarkAsNoData();
            await sensorRepository.UpdateAsync(sensor, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
