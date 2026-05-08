using Contracts.Events.SensorEvents;
using MassTransit;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.Specifications;

namespace Telemetry.Application.Services;

public class SensorStateCheckerService(
    ISensorRepository sensorRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork)
    : ISensorStateCheckerService
{
    public async Task CheckStateAndNotify(CancellationToken cancellationToken)
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
            return;
        }

        var affectedSensors = new List<SensorEntity>();

        foreach (var sensor in sensors)
        {
            sensor.MarkAsNoData();

            await sensorRepository.UpdateAsync(sensor, cancellationToken);
            affectedSensors.Add(sensor);
        }

        if (affectedSensors.Count != 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var sensor in affectedSensors)
            {
                await publishEndpoint.Publish(new SensorNoDataEvent
                {
                    SensorId = sensor.Id,
                    State = sensor.State,
                    LastSeenAt = sensor.UpdatedAt,
                }, cancellationToken);
            }
        }
    }
}
