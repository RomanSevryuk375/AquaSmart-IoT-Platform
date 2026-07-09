using Contracts.Results;
using MediatR;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.DeleteReminder;

public sealed class DeleteReminderHandler(
    IReminderRepository reminderRepository) : IRequestHandler<DeleteReminderCommand, Result>
{
    public async Task<Result> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
    {
        await reminderRepository.DeleteAsync(request.ReminderId, cancellationToken);

        return Result.Success();
    }
}
