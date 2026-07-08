using Contracts.Abstractions;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.CompleteReminder;

public sealed record CompleteReminderCommand
    : ICommand, IReminderBoundRequest
{
    public Guid ReminderId { get; init; }
}
