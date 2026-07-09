namespace Device.Infrastructure.Extensions;

public sealed record BackgroundJobsOptions
{
    public const string SectionName = "BackgroundJobs";
    public int OutboxProcessorIntervalSeconds { get; init; } = 1;
    public int OfflineCheckerIntervalSeconds { get; init; } = 60;
    public int DeleteCompletedCommandsIntervalHours { get; init; } = 3;
}
