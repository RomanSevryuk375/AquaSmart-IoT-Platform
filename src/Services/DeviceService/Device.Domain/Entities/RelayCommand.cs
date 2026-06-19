namespace Device.Domain.Entities;

public sealed class RelayCommand : IEntity
{
    private RelayCommand(
        Guid id,
        Guid controllerId,
        Guid relayId,
        RuleActionEnum action,
        CommandStatusEnum status,
        DateTime? expireAt,
        int attemptCount,
        DateTime? processedAt,
        string? errorMessage,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        RelayId = relayId;
        Action = action;
        Status = status;
        ExpireAt = expireAt;
        AttemptCount = attemptCount;
        ProcessedAt = processedAt;
        ErrorMessage = errorMessage;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid RelayId { get; private set; }
    public RuleActionEnum Action { get; private set; }
    public CommandStatusEnum Status { get; private set; }
    public DateTime? ExpireAt { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<RelayCommand> Create(
        Guid id,
        Guid controllerId,
        Guid relayId,
        RuleActionEnum action,
        DateTime? expireAt)
    {
        var command = new RelayCommand(
            id,
            controllerId,
            relayId,
            action,
            status: CommandStatusEnum.Pending,
            expireAt ?? DateTime.UtcNow.AddMinutes(5),
            attemptCount: 0,
            processedAt: null,
            errorMessage: null,
            createdAt: DateTime.UtcNow);

        return Result<RelayCommand>.Success(command);
    }

    public void MarkAsSent()
    {
        if (Status == CommandStatusEnum.Completed || 
            Status == CommandStatusEnum.Failed)
        {
            return;
        }

        ProcessedAt = DateTime.UtcNow;
        AttemptCount++;
        Status = CommandStatusEnum.Sent;
    }

    public void MarkAsCompleted()
    {
        if (Status == CommandStatusEnum.Completed)
        {
            return;
        }

        Status = CommandStatusEnum.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (Status == CommandStatusEnum.Failed)
        {
            return;
        }

        ErrorMessage = errorMessage;
        Status = CommandStatusEnum.Failed;
    }
}
