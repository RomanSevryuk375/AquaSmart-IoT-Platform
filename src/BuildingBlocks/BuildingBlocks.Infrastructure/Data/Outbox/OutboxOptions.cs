namespace BuildingBlocks.Infrastructure.Data.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int IntervalSeconds { get; set; } = 1;
    public int BatchSize { get; set; } = 50;
    public int MaxRetries { get; set; } = 5;
    public int RetentionDays { get; set; } = 7;
    public int CleanupIntervalHours { get; set; } = 24;
}
