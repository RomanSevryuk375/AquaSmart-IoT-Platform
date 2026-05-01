using Contracts.Exceptions;
using Notification.Application.DTOs.Reminder;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.Domain.SpecificationParams;
using Notification.Domain.Specifications;

namespace Notification.Application.Services;

public class ReminderService(
    IAquariumRepository aquariumRepository,
    IUserRepository userRepository,
    IReminderRepository reminderRepository,
    IUnitOfWork unitOfWork) : IReminderService
{
    public async Task<Guid> AddReminderAsync(
        ReminderRequestDto request, 
        CancellationToken cancellationToken)
    {
        var existingAquarium = await aquariumRepository
            .GetByIdAsync(request.AquariumId, cancellationToken)
            ?? throw new NotFoundException($"Aquarium {request.AquariumId} not found");

        var existingUser = await userRepository
            .GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User {request.UserId} not found");

        var (reminder, errors) = ReminderEntity.Create(
            request.UserId,
            request.AquariumId,
            request.TaskName,
            request.IntervalDays);

        if (reminder is null)
        {
            throw new DomainValidationException(
                $"Failed to create {nameof(ReminderEntity)}: {string.Join(", ", errors)}");
        }

        var result = await reminderRepository.AddAsync(reminder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task DeleteReminderAsync(
        Guid id, 
        CancellationToken cancellationToken)
    {
        await reminderRepository.DeleteAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReminderResponseDto>> GetAllRemindersAsync(
        ReminderFilterDto filter, 
        int? skip, 
        int? take, 
        CancellationToken cancellationToken)
    {
        var specification = new ReminderFilterSpecification(
            new ReminderSpecificationParams
            {
                UserId = filter.UserId,
                EcosystemId = filter.AquariumId,
                SearchTerm = filter.SearchTerm,
                LastDoneAtFrom = filter.LastDoneAtFrom,
                LastDoneAtTo = filter.LastDoneAtTo,
                NextDueAtFrom = filter.NextDueAtFrom,
                NextDueAtTo = filter.NextDueAtTo,
            });

        var reminders = await reminderRepository.GetAllAsync(
            specification, 
            skip, 
            take, 
            cancellationToken);

        return reminders.Select(r => new ReminderResponseDto
        {
            Id = r.Id,
            UserId = r.UserId,
            AquariumId = r.EcosystemId,
            TaskName = r.TaskName,
            IntervalDays = r.IntervalDays,
            NextDueAt = r.NextDueAt,
            CreatedAt = r.CreatedAt,
        }).ToList();
    }

    public async Task<ReminderResponseDto> GetReminderByIdAsync(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository
            .GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"{nameof(ReminderEntity)} not found");

        return new ReminderResponseDto
        {
            Id = reminder.Id,
            UserId = reminder.UserId,
            AquariumId = reminder.EcosystemId,
            TaskName = reminder.TaskName,
            IntervalDays = reminder.IntervalDays,
            NextDueAt = reminder.NextDueAt,
            CreatedAt = reminder.CreatedAt,
        };
    }

    public async Task ReminderCompleteTaskAsync(
        Guid id, CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository
            .GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"{nameof(ReminderEntity)} not found");

        reminder.CompleteTask();

        await reminderRepository.UpdateAsync(reminder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateReminderAsync(
        Guid id, 
        ReminderUpdateRequestDto request, 
        CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository
            .GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"{nameof(ReminderEntity)} not found");

        reminder.UpdateSchedule(
            request.TaskName,
            request.IntervalDays);

        await reminderRepository.UpdateAsync(reminder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
