using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Domain.Events.RelayCommandEvents;

namespace Device.Domain.Entities;

public sealed class RelayCommand : AggregateRoot, IEntity
{
    private RelayCommand(
        Guid id,
        Guid controllerId,
        Guid relayId,
        bool targetState,
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
        TargetState = targetState;
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
    public bool TargetState { get; private set; }
    public CommandStatus Status { get; private set; }
    public DateTime? ExpireAt { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsTerminal => Status is CommandStatus.Completed or CommandStatus.Failed or CommandStatus.Cancelled or CommandStatus.Expired;

    public static Result<RelayCommand> Create(
        Guid id,
        Guid controllerId,
        Guid relayId,
        bool targetState,
        DateTime? expireAt)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "RelayCommand Id", errors);
        DomainGuard.CheckNotEmpty(controllerId, nameof(ControllerId), errors);
        DomainGuard.CheckNotEmpty(relayId, nameof(RelayId), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<RelayCommand>(errors);
        }

        DateTime createdAt = DateTime.UtcNow;
        DateTime resolvedExpireAt = expireAt ?? createdAt.AddMinutes(RelayCommandConstants.DefaultExpiryMinutes);

        var command = new RelayCommand(
            id,
            controllerId,
            relayId,
            targetState,
            status: CommandStatus.Pending,
            resolvedExpireAt,
            attemptCount: 0,
            processedAt: null,
            errorMessage: null,
            createdAt: createdAt);

        command.RaiseEvent(new RelayCommandCreatedDomainEvent
        {
            CommandId = command.Id,
            ControllerId = command.ControllerId,
            RelayId = command.RelayId,
            TargetState = command.TargetState,
            ExpireAt = command.ExpireAt,
            CreatedAt = command.CreatedAt,
        });

        return Result<RelayCommand>.Success(command);
    }

    public void MarkAsSent()
    {
        if (IsTerminal)
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
        if (IsTerminal)
        {
            return;
        }

        Status = CommandStatus.Completed;
        ProcessedAt = DateTime.UtcNow;

        IncrementVersion();
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (IsTerminal)
        {
            return;
        }

        ErrorMessage = errorMessage;
        Status = CommandStatus.Failed;
        ProcessedAt = DateTime.UtcNow;

        IncrementVersion();
    }

    public Result Cancel()
    {
        if (IsTerminal)
        {
            return Result.Failure(Error.Conflict<RelayCommand>(
                $"Relay command {Id} cannot be cancelled from terminal status {Status}."));
        }

        Status = CommandStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;

        IncrementVersion();

        return Result.Success();
    }

    public Result Expire()
    {
        if (IsTerminal)
        {
            return Result.Failure(Error.Conflict<RelayCommand>(
                $"Relay command {Id} cannot be expired from terminal status {Status}."));
        }

        Status = CommandStatus.Expired;
        ProcessedAt = DateTime.UtcNow;

        IncrementVersion();

        return Result.Success();
    }
}
