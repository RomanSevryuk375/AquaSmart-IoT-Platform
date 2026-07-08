using Contracts.Results;
using MediatR;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.CompleteReminder;

public sealed class CompleteReminderHandler(IReminderRepository reminderRepository)
    : IRequestHandler<CompleteReminderCommand, Result>
{
    public async Task<Result> Handle(CompleteReminderCommand request, CancellationToken cancellationToken)
    {
        Reminder? reminder = await reminderRepository.GetByIdAsync(request.ReminderId, cancellationToken);

        reminder!.CompleteTask();

        return Result.Success();
    }
}
