using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Sensors.Commands.SyncSensorName;

public sealed class SyncSensorNameHandler(ISensorRepository sensorRepository)
    : IRequestHandler<SyncSensorNameCommand, Result>
{
    public async Task<Result> Handle(SyncSensorNameCommand request, CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
        if (existingSensor is null)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"Sensor {request.SensorId} not found."));
        }

        Result updateResult = existingSensor.SetName(request.Name);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        return Result.Success();
    }
}
