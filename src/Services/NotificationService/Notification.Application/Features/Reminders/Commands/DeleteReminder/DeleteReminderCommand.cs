using Contracts.Abstractions;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.DeleteReminder;

public sealed record DeleteReminderCommand : ICommand, IReminderBoundRequest
{
    public Guid ReminderId { get; init; }
}
