using Contracts.Abstractions;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.Reminders.Commands.CreateReminder;

public sealed record CreateReminderCommand
    : ICommand<Guid>, IEcosystemBoundRequest
{
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public string TaskName { get; init; } = string.Empty;
    public int IntervalDays { get; init; }
}
