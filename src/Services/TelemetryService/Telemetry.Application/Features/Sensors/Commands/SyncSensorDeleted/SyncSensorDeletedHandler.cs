using Contracts.Results;
using MediatR;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.Sensors.Commands.SyncSensorDeleted;

internal sealed class SyncSensorDeletedHandler(ISensorRepository sensorRepository)
    : IRequestHandler<SyncSensorDeletedCommand, Result>
{
    public async Task<Result> Handle(SyncSensorDeletedCommand request, CancellationToken cancellationToken)
    {
        await sensorRepository.DeleteAsync(request.SensorId, cancellationToken);
        return Result.Success();
    }
}
