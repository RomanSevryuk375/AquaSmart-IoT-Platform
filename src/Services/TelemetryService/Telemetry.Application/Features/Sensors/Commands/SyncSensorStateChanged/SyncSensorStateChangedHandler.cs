using Contracts.Results;
using MediatR;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorStateChanged;

internal sealed class SyncSensorStateChangedHandler(ISensorRepository sensorRepository)
    : IRequestHandler<SyncSensorStateChangedCommand, Result>
{
    public async Task<Result> Handle(SyncSensorStateChangedCommand request, CancellationToken cancellationToken)
    {
        Sensor? sensor = await sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
        if (sensor is null)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"Sensor {request.SensorId} not found."));
        }

        sensor.SetState(request.State);

        return Result.Success();
    }
}
