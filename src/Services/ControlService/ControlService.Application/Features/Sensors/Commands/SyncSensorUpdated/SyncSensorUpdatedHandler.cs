using AutoMapper;
using Contracts.Results;
using Control.Application.Features.Sensors.Commands.SyncSensorCreated;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Sensors.Commands.SyncSensorUpdated;

public sealed class SyncSensorUpdatedHandler(
    ISensorRepository sensorRepository,
    ISender sender,
    IMapper mapper) : IRequestHandler<SyncSensorUpdatedCommand, Result>
{
    public async Task<Result> Handle(SyncSensorUpdatedCommand request, CancellationToken cancellationToken)
    {
        Sensor? sensor = await sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);

        if (sensor is null)
        {
            SyncSensorCreatedCommand createCommand = mapper.Map<SyncSensorCreatedCommand>(request);

            return await sender.Send(createCommand, cancellationToken);
        }

        sensor.SetType(request.Type);
        sensor.SetState(request.State);

        Result nameResult = sensor.SetName(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        return Result.Success();
    }
}
