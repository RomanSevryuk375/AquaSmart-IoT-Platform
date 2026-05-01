using AutoMapper;
using Contracts.Results;
using FluentValidation;
using Notification.Application.DTOs.Reminder;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.Domain.SpecificationParams;
using Notification.Domain.Specifications;

namespace Notification.Application.Services;

public class ReminderService(
    IEcosystemRepository ecosystemRepository,
    IReminderRepository reminderRepository,
    IUserContext userContext,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IValidator<ReminderRequestDto> validator) : IReminderService
{
    public async Task<Result<Guid>> AddReminderAsync(
        ReminderRequestDto request,
        CancellationToken cancellationToken)
    {
        validator.ValidateAndThrow(request);

        var ecosystem = await ecosystemRepository
            .GetByIdAsync(request.EcosystemId, cancellationToken);

        if (ecosystem is null || ecosystem.UserId != userContext.UserId)
        {
            return Result<Guid>.Failure(Error.NotFound(
                "Ecosystem.NotFound",
                $"Ecosystem {request.EcosystemId} not found"));
        }

        var (reminder, errors) = ReminderEntity.Create(
            userContext.UserId,
            request.EcosystemId,
            request.TaskName,
            request.IntervalDays);

        if (reminder is null)
        {
            return Result<Guid>.Failure(Error.Validation(
                "Reminder.Invalid",
                $"Failed to create {nameof(ReminderEntity)}: {string.Join(", ", errors)}"));
        }

        var result = await reminderRepository.AddAsync(reminder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ReminderResponseDto>>> GetAllRemindersAsync(
        ReminderFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var specification = new ReminderFilterSpecification(
            new ReminderSpecificationParams
            {
                UserId = userContext.UserId,
                EcosystemId = filter.EcosystemId,
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

        return Result<IReadOnlyList<ReminderResponseDto>>.Success(
            mapper.Map<IReadOnlyList<ReminderResponseDto>>(reminders));
    }

    public async Task<Result<ReminderResponseDto>> GetReminderByIdAsync(
        Guid reminderId,
        CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository.GetByIdAsync(reminderId, cancellationToken);

        if (reminder is null ||
            reminder.UserId != userContext.UserId)
        {
            return Result<ReminderResponseDto>.Failure(Error.NotFound(
                "Reminder.NotFound",
                $"Reminder {reminderId} not found"));
        }

        return Result<ReminderResponseDto>.Success(
            mapper.Map<ReminderResponseDto>(reminder));
    }

    public async Task<Result> UpdateReminderAsync(
        Guid reminderId,
        ReminderUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository
            .GetByIdAsync(reminderId, cancellationToken);

        if (reminder is null ||
            reminder.UserId != userContext.UserId)
        {
            return Result.Failure(Error.NotFound(
                "Reminder.NotFound",
                $"Reminder {reminderId} not found"));
        }

        reminder.UpdateSchedule(request.TaskName, request.IntervalDays);

        await reminderRepository.UpdateAsync(reminder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ReminderCompleteTaskAsync(
        Guid reminderId,
        CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository
            .GetByIdAsync(reminderId, cancellationToken);

        if (reminder is null ||
            reminder.UserId != userContext.UserId)
        {
            return Result.Failure(Error.NotFound(
                "Reminder.NotFound",
                $"Reminder {reminderId} not found"));
        }

        reminder.CompleteTask();

        await reminderRepository.UpdateAsync(reminder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteReminderAsync(
        Guid reminderId,
        CancellationToken cancellationToken)
    {
        var reminder = await reminderRepository
            .GetByIdAsync(reminderId, cancellationToken);

        if (reminder is null ||
            reminder.UserId != userContext.UserId)
        {
            return Result.Failure(Error.NotFound(
                "Reminder.NotFound",
                $"Reminder {reminderId} not found"));
        }

        await reminderRepository.DeleteAsync(reminderId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}