using AutoMapper;
using Contracts.Results;
using MediatR;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorCreated;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorUpdated;

internal sealed class SyncSensorUpdatedHandler(
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
        Result updateResult = sensor.Update(request.ControllerId, request.Type, request.Unit);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        sensor.SetState(request.State);
        Result nameResult = sensor.SetName(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        return Result.Success();
    }
}
