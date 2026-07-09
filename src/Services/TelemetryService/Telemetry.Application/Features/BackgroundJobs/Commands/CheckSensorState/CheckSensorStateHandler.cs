using Contracts.Results;
using MediatR;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.CheckSensorState;

internal sealed class CheckSensorStateHandler(
    ISensorRepository sensorRepository) : IRequestHandler<CheckSensorStateCommand, Result>
{
    private const int OfflineThresholdMinutes = -5;

    public async Task<Result> Handle(CheckSensorStateCommand request, CancellationToken cancellationToken)
    {
        DateTime offlineThreshold = DateTime.UtcNow.AddMinutes(OfflineThresholdMinutes);

        IReadOnlyList<Sensor> sensors = await sensorRepository.GetDelayedSensors(
            offlineThreshold, cancellationToken);

        if (sensors.Count == 0)
        {
            return Result.Success();
        }

        foreach (Sensor sensor in sensors)
        {
            sensor.MarkAsNoData();
        }

        return Result.Success();
    }
}
