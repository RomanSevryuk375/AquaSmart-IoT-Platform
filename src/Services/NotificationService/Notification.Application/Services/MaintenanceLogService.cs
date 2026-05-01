using Contracts.Exceptions;
using Notification.Application.DTOs.MaintenanceLog;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.Domain.SpecificationParams;
using Notification.Domain.Specifications;

namespace Notification.Application.Services;

public class MaintenanceLogService(
    IMaintenanceLogRepository logRepository,
    IUserRepository userRepository,
    IAquariumRepository aquariumRepository,
    IUnitOfWork unitOfWork) : IMaintenanceLogService
{
    public async Task<Guid> AddLogAsync(
        MaintenanceLogRequestDto request, 
        CancellationToken cancellationToken)
    {
        var existingAquarium = await aquariumRepository
            .GetByIdAsync(request.AquariumId, cancellationToken)
            ?? throw new NotFoundException($"Aquarium {request.AquariumId} not found");

        var existingUser = await userRepository
            .GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User {request.UserId} not found");

        var (log, errors) = MaintenanceLogEntity.Create(
            request.UserId,
            request.AquariumId,
            request.ActionDate,
            request.PhLevel,
            request.KhLevel,
            request.No3Level,
            request.Notes);

        if (log is null)
        {
            throw new DomainValidationException(
                $"Failed to create {nameof(MaintenanceLogEntity)}: {string.Join(", ", errors)}");
        }

        var result = await logRepository.AddAsync(log, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<MaintenanceLogResponseDto>> GetAllLogs(
        MaintenanceLogFilterDto filter, 
        int? skip, 
        int? take, 
        CancellationToken cancellationToken)
    {
        var specification = new MaintenanceLogFilterSpecification(
            new MaintenanceLogSpecificationParams
            {
                UserId = filter.UserId,
                EcosystemId = filter.AquariumId,
                ActionDateFrom = filter.ActionDateFrom,
                ActionDateTo = filter.ActionDateTo,
            });

        var logs = await logRepository.GetAllAsync(
            specification, 
            skip, 
            take, 
            cancellationToken);

        return logs.Select(log => new MaintenanceLogResponseDto
        {
            Id = log.Id,
            UserId = log.UserId,
            AquariumId = log.EcosystemId,
            ActionDate = log.ActionDate,
            PhLevel = log.PhLevel,
            KhLevel = log.KhLevel,
            No3Level = log.No3Level,
            Notes = log.Notes,
            CreatedAt = log.CreatedAt
        }).ToList();
    }

    public async Task<MaintenanceLogResponseDto> GetLogById(Guid id, CancellationToken cancellationToken)
    {
        var log = await logRepository
            .GetByIdAsync(id, cancellationToken) 
            ?? throw new NotFoundException($"{nameof(MaintenanceLogEntity)} not found");

        return new MaintenanceLogResponseDto
        {
            Id = log.Id,
            UserId = log.UserId,
            AquariumId = log.EcosystemId,
            ActionDate = log.ActionDate,
            PhLevel = log.PhLevel,
            KhLevel = log.KhLevel,
            No3Level = log.No3Level,
            Notes = log.Notes,
            CreatedAt = log.CreatedAt
        };
    }
}
