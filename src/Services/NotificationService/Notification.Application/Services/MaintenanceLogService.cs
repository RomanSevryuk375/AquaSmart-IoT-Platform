using AutoMapper;
using Contracts.Results;
using FluentValidation;
using MassTransit;
using Notification.Application.DTOs.MaintenanceLog;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;


namespace Notification.Application.Services;

public class MaintenanceLogService(
    IMaintenanceLogRepository logRepository,
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    IMapper mapper,
    IValidator<MaintenanceLogRequestDto> validator) : IMaintenanceLogService
{
    public async Task<Result<Guid>> AddLogAsync(
        MaintenanceLogRequestDto request,
        CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);

        Ecosystem? existingEcosystem = await ecosystemRepository
            .GetByIdAsync(request.EcosystemId, cancellationToken);

        if (existingEcosystem is null)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"Ecosystem {request.EcosystemId} not found"));
        }

        if (existingEcosystem.UserId != userContext.UserId)
        {
            return Result<Guid>
                .Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        Result<MaintenanceLog>? logResult = MaintenanceLog.Create(
            NewId.NextGuid(),
            userContext.UserId,
            request.EcosystemId,
            request.ActionDate,
            request.Metrics,
            request.Notes);

        if (logResult.IsFailure)
        {
            return Result<Guid>.Failure(logResult.Error);
        }

        Guid result = await logRepository.AddAsync(logResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result<MaintenanceLogResponseDto>> GetLogById(
        Guid logId,
        CancellationToken cancellationToken)
    {
        MaintenanceLog? log = await logRepository
            .GetByIdAsync(logId, cancellationToken);

        if (log is null ||
            log.UserId != userContext.UserId)
        {
            return Result<MaintenanceLogResponseDto>
                .Failure(Error.NotFound(
                    "Log.NotFound",
                    $"Log {logId} not found"));
        }

        return Result<MaintenanceLogResponseDto>
            .Success(mapper.Map<MaintenanceLogResponseDto>(log));
    }
}
