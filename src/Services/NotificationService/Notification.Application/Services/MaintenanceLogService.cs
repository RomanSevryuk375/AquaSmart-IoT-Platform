using AutoMapper;
using Contracts.Results;
using FluentValidation;
using Notification.Application.DTOs.MaintenanceLog;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.Domain.SpecificationParams;
using Notification.Domain.Specifications;


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

        var existingEcosystem = await ecosystemRepository
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

        var (log, errors) = MaintenanceLogEntity.Create(
            userContext.UserId,
            request.EcosystemId,
            request.ActionDate,
            request.Metrics,
            request.Notes);

        if (log is null)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Log.Invalid",
                    $"Failed to create {nameof(MaintenanceLogEntity)}: {string.Join(", ", errors)}"));
        }

        var result = await logRepository.AddAsync(log, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result<IReadOnlyList<MaintenanceLogResponseDto>>> GetAllLogs(
        MaintenanceLogFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new MaintenanceLogFilterSpecification(
            new MaintenanceLogSpecificationParams
            {
                UserId = userContext.UserId,
                EcosystemId = filter.EcosystemId,
                ActionDateFrom = filter.ActionDateFrom,
                ActionDateTo = filter.ActionDateTo,
            });

        var logs = await logRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        return Result<IReadOnlyList<MaintenanceLogResponseDto>>.Success(
            mapper.Map<IReadOnlyList<MaintenanceLogResponseDto>>(logs));
    }

    public async Task<Result<MaintenanceLogResponseDto>> GetLogById(
        Guid logId, 
        CancellationToken cancellationToken)
    {
        var log = await logRepository
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
