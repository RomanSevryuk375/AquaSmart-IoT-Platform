using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Sensors.Commands.SyncSensorState;

public sealed class SyncSensorStateHandler(ISensorRepository sensorRepository)
    : IRequestHandler<SyncSensorStateCommand, Result>
{
    public async Task<Result> Handle(SyncSensorStateCommand request, CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
        if (existingSensor is null)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"Sensor {request.SensorId} not found."));
        }

        existingSensor.SetState(request.State);

        return Result.Success();
    }
}
