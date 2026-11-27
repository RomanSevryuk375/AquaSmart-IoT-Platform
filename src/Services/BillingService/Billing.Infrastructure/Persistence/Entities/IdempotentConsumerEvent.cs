namespace Billing.Infrastructure.Persistence.Entities;


public sealed class IdempotentConsumerEvent
{
    public const int MaxEventIdLength = 128;
    public const int MaxConsumerNameLength = 128;

    public string EventId { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
