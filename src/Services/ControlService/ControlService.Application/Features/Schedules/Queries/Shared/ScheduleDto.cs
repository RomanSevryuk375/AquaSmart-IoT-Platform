// Ignore Spelling: Dto Cron

namespace Control.Application.Features.Schedules.Queries.Shared;

public sealed record ScheduleDto
{
    public Guid Id { get; init; }
    public Guid EcosystemId { get; init; }
    public Guid RelayId { get; init; }
    public string CronExpression { get; init; } = string.Empty;
    public double DurationMin { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsFadeMode { get; init; }
    public DateTime CreatedAt { get; init; }
}
