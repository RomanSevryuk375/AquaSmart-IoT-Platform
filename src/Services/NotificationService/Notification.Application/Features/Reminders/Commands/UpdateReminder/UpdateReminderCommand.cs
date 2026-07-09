using Contracts.Abstractions;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.UpdateReminder;

public sealed record UpdateReminderCommand : ICommand, IReminderBoundRequest
{
    public Guid ReminderId { get; init; }
    public string TaskName { get; init; } = string.Empty;
    public int IntervalDays { get; init; }
}
