using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Sensors.Commands.SyncSensorDeleted;

public sealed class SyncSensorDeletedHandler(ISensorRepository sensorRepository)
    : IRequestHandler<SyncSensorDeletedCommand, Result>
{
    public async Task<Result> Handle(SyncSensorDeletedCommand request, CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
        if (existingSensor is null)
        {
            return Result.Success();
        }

        await sensorRepository.DeleteAsync(request.SensorId, cancellationToken);

        return Result.Success();
    }
}
