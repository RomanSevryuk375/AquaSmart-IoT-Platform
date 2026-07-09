using Contracts.Results;
using MassTransit;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.CreateReminder;

public sealed class CreateReminderHandler(IReminderRepository reminderRepository)
    : IRequestHandler<CreateReminderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
    {
        Result<Reminder> result = Reminder.Create(
            reminderId: NewId.NextGuid(),
            request.UserId,
            request.EcosystemId,
            request.TaskName,
            request.IntervalDays);
        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error);
        }

        await reminderRepository.AddAsync(result.Value, cancellationToken);

        return Result<Guid>.Success(result.Value.Id);
    }
}
