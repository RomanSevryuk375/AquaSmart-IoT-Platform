namespace Device.Domain.Entities;

public sealed class RelayCommand : AggregateRoot, IEntity
{
    private RelayCommand(
        Guid id,
        Guid controllerId,
        Guid relayId,
        bool targeState,
        CommandStatus status,
        DateTime? expireAt,
        int attemptCount,
        DateTime? processedAt,
        string? errorMessage,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        RelayId = relayId;
        TargeState = targeState;
        Status = status;
        ExpireAt = expireAt;
        AttemptCount = attemptCount;
        ProcessedAt = processedAt;
        ErrorMessage = errorMessage;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private RelayCommand() { }
#pragma warning restore CS8618 

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid RelayId { get; private set; }
    public bool TargeState { get; private set; }
    public CommandStatus Status { get; private set; }
    public DateTime? ExpireAt { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<RelayCommand> Create(
        Guid id,
        Guid controllerId,
        Guid relayId,
        bool targetState,
        DateTime? expireAt)
    {
        var command = new RelayCommand(
            id,
            controllerId,
            relayId,
            targetState,
            status: CommandStatus.Pending,
            expireAt ?? DateTime.UtcNow.AddMinutes(5),
            attemptCount: 0,
            processedAt: null,
            errorMessage: null,
            createdAt: DateTime.UtcNow);

        return Result<RelayCommand>.Success(command);
    }

    public void MarkAsSent()
    {
        if (Status == CommandStatus.Completed ||
            Status == CommandStatus.Failed)
        {
            return;
        }

        ProcessedAt = DateTime.UtcNow;
        AttemptCount++;
        Status = CommandStatus.Sent;

        IncrementVersion();
    }

    public void MarkAsCompleted()
    {
        if (Status == CommandStatus.Completed)
        {
            return;
        }

        Status = CommandStatus.Completed;
        ProcessedAt = DateTime.UtcNow;

        IncrementVersion();
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (Status == CommandStatus.Failed)
        {
            return;
        }

        ErrorMessage = errorMessage;
        Status = CommandStatus.Failed;

        IncrementVersion();
    }
}
