using Contracts.Results;
using MediatR;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorNameChanged;

internal sealed class SyncSensorNameChangedHandler(ISensorRepository sensorRepository)
    : IRequestHandler<SyncSensorNameChangedCommand, Result>
{
    public async Task<Result> Handle(SyncSensorNameChangedCommand request, CancellationToken cancellationToken)
    {
        Sensor? sensor = await sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
        if (sensor is null)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"Sensor {request.SensorId} not found."));
        }

        Result nameResult = sensor.SetName(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        return Result.Success();
    }
}
