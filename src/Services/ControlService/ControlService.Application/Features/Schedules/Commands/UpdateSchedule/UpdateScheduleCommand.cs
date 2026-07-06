// Ignore Spelling: Cron

using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.Schedules.Commands.UpdateSchedule;

public sealed record UpdateScheduleCommand
    : ICommand, IScheduleBoundRequest
{
    public Guid ScheduleId { get; init; }
    public string CronExpression { get; init; } = string.Empty;
    public double DurationMin { get; init; }
    public bool IsFadeMode { get; init; }
    public bool IsEnabled { get; init; }
}
