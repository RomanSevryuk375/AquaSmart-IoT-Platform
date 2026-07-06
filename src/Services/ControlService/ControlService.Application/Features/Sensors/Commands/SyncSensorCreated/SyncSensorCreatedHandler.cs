using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Sensors.Commands.SyncSensorCreated;

public sealed class SyncSensorCreatedHandler(
    ISensorRepository sensorRepository,
    IEcosystemRepository ecosystemRepository) : IRequestHandler<SyncSensorCreatedCommand, Result>
{
    public async Task<Result> Handle(SyncSensorCreatedCommand request, CancellationToken cancellationToken)
    {
        if (await sensorRepository.GetByIdAsync(request.SensorId, cancellationToken) is not null)
        {
            return Result.Success();
        }

        Ecosystem? ecosystem = await ecosystemRepository.GetByControllerIdAsync(
            request.ControllerId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem with controller {request.ControllerId} not found."));
        }

        Result<Sensor> createResult = Sensor.Create(
            request.SensorId, request.ControllerId, ecosystem.Id,
            request.Name, request.State, request.Type, request.CreatedAt);
        if (createResult.IsFailure)
        {
            return Result.Failure(createResult.Error);
        }

        await sensorRepository.AddAsync(createResult.Value, cancellationToken);

        return Result.Success();
    }
}
