namespace BuildingBlocks.Domain.Enums;

public enum CommandStatus
{
    Pending = 1,
    Sent = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Expired = 6,
}
