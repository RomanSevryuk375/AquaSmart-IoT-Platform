using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.Specifications;

namespace Telemetry.Application.Services;

public class SensorStateCheckerService(
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork)
    : ISensorStateCheckerService
{
    public async Task<Result> CheckStateAndNotify(CancellationToken cancellationToken)
    {
        var offlineThreshold = DateTime.UtcNow.AddMinutes(-5);
        var specification =
            new SensorIsDelayedSpecification(offlineThreshold);

        var sensors = await sensorRepository.GetAllAsync(
            specification,
            null,
            null,
            cancellationToken);

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
