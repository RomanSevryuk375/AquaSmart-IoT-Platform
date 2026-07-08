using AutoMapper;
using Contracts.Results;
using FluentValidation;
using MassTransit;
using Notification.Application.DTOs.Reminder;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

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

        Ecosystem? ecosystem = await ecosystemRepository
            .GetByIdAsync(request.EcosystemId, cancellationToken);

        if (ecosystem is null || ecosystem.UserId != userContext.UserId)
        {
            return Result<Guid>.Failure(Error.NotFound(
                "Ecosystem.NotFound",
                $"Ecosystem {request.EcosystemId} not found"));
        }

        Result<Reminder> reminderResult = Reminder.Create(
            NewId.NextGuid(),
            userContext.UserId,
            request.EcosystemId,
            request.TaskName,
            request.IntervalDays);

        if (reminderResult.IsFailure)
        {
            return Result<Guid>.Failure(reminderResult.Error);
        }

        Guid result = await reminderRepository.AddAsync(reminderResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result);
    }

    public async Task<Result<ReminderResponseDto>> GetReminderByIdAsync(
        Guid reminderId,
        CancellationToken cancellationToken)
    {
        Reminder? reminder = await reminderRepository.GetByIdAsync(reminderId, cancellationToken);

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
        Reminder? reminder = await reminderRepository
            .GetByIdAsync(reminderId, cancellationToken);

        if (reminder is null ||
            reminder.UserId != userContext.UserId)
        {
            return Result.Failure(Error.NotFound(
                "Reminder.NotFound",
                $"Reminder {reminderId} not found"));
        }

        reminder.UpdateSchedule(request.TaskName, request.IntervalDays);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ReminderCompleteTaskAsync(
        Guid reminderId,
        CancellationToken cancellationToken)
    {
        Reminder? reminder = await reminderRepository
            .GetByIdAsync(reminderId, cancellationToken);

        if (reminder is null ||
            reminder.UserId != userContext.UserId)
        {
            return Result.Failure(Error.NotFound(
                "Reminder.NotFound",
                $"Reminder {reminderId} not found"));
        }

        reminder.CompleteTask();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteReminderAsync(
        Guid reminderId,
        CancellationToken cancellationToken)
    {
        Reminder? reminder = await reminderRepository
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
