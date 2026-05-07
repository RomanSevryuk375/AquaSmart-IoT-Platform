using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;

namespace Device.Domain.Entities;

public sealed class RelayCommandsQueueEntity : IEntity
{
    private RelayCommandsQueueEntity(
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

    public static Result<RelayCommandsQueueEntity> Create(
        Guid controllerId,
        Guid relayId,
        RuleActionEnum action,
        DateTime? expireAt)
    {
        var errors = new List<string>();

        if (controllerId == Guid.Empty)
        {
            errors.Add("controllerId must not be empty.");
        }

        if (relayId == Guid.Empty)
        {
            errors.Add("relayId must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result<RelayCommandsQueueEntity>.Failure(
                Error.Validation(
                    "Command.Invalid",
                    string.Join("; ", errors)));
        }

        var command = new RelayCommandsQueueEntity(
            Guid.NewGuid(),
            controllerId,
            relayId,
            action,
            CommandStatusEnum.Pending,
            expireAt ?? DateTime.UtcNow.AddMinutes(5),
            0,
            null,
            null,
            DateTime.UtcNow);

        return Result<RelayCommandsQueueEntity>.Success(command);
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
