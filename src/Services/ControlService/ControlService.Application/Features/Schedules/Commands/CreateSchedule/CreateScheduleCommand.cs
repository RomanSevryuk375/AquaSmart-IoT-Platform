// Ignore Spelling: Cron

using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.Schedules.Commands.CreateSchedule;

public sealed record CreateScheduleCommand
    : ICommand<Guid>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public Guid RelayId { get; init; }
    public string CronExpression { get; init; } = string.Empty;
    public double DurationMin { get; init; }
    public bool IsFadeMode { get; init; }
    public bool IsEnabled { get; init; }
}
