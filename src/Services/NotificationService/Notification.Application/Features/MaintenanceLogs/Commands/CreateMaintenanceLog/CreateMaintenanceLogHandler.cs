using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.MaintenanceLogs.Commands.CreateMaintenanceLog;

public sealed class CreateMaintenanceLogHandler(IMaintenanceLogRepository logRepository)
    : IRequestHandler<CreateMaintenanceLogCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMaintenanceLogCommand request, CancellationToken cancellationToken)
    {
        Result<MaintenanceLog> logResult = MaintenanceLog.Create(
            logId: NewId.NextGuid(),
            request.UserId,
            request.EcosystemId,
            request.ActionDate,
            request.Metrics,
            request.Notes);
        if (logResult.IsFailure)
        {
            return Result<Guid>.Failure(logResult.Error);
        }

        await logRepository.AddAsync(logResult.Value, cancellationToken);


        return Result<Guid>.Success(logResult.Value.Id);
    }
}
