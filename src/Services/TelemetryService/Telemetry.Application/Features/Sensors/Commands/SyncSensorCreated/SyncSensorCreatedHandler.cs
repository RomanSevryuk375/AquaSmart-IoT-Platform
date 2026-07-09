using Contracts.Results;
using MediatR;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorCreated;

internal sealed class SyncSensorCreatedHandler(
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
                $"Ecosystem for controller {request.ControllerId} not found."));
        }

        Result<Sensor> sensorResult = Sensor.Create(
            request.SensorId, request.ControllerId, ecosystem.Id,
            request.Name, request.Type, request.State,
            request.Unit, lastValue: 0.0, updatedAt: DateTime.UtcNow,
            createdAt: request.CreatedAt);
        if (sensorResult.IsFailure)
        {
            return Result.Failure(sensorResult.Error);
        }

        await sensorRepository.AddAsync(sensorResult.Value, cancellationToken);

        return Result.Success();
    }
}
