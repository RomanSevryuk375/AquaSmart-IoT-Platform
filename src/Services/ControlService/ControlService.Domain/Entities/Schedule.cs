// Ignore Spelling: Cron Validator

using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Control.Domain.Interfaces;
using Control.Domain.ValueObjects;

namespace Control.Domain.Entities;

public sealed class Schedule : AggregateRoot, IEntity
{
    private Schedule(
        Guid id,
        Guid ecosystemId,
        Guid relayId,
        CronSchedule cronExpression,
        double durationMin,
        bool isFadeMode,
        bool isEnabled,
        DateTime createdAt)
    {
        Id = id;
        EcosystemId = ecosystemId;
        RelayId = relayId;
        CronExpression = cronExpression;
        DurationMin = durationMin;
        IsFadeMode = isFadeMode;
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private Schedule() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Guid RelayId { get; private set; }
    public CronSchedule CronExpression { get; private set; }
    public double DurationMin { get; private set; }
    public bool IsFadeMode { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Schedule> Create(
        Guid scheduleId,
        Guid ecosystemId,
        Guid relayId,
        string rawCronExpression,
        ICronValidator cronValidator,
        double durationMin,
        bool isFadeMode,
        bool isEnabled)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(scheduleId, "Schedule Id", errors);
        DomainGuard.CheckNotEmpty(ecosystemId, nameof(EcosystemId), errors);
        DomainGuard.CheckNotEmpty(relayId, nameof(RelayId), errors);
        CronSchedule? cron = DomainGuard.Collect(
            CronSchedule.Create(rawCronExpression, cronValidator), errors);
        if (durationMin <= ControlConstants.MinDuration)
        {
            errors.Add(ControlValidationMessages.DurationMinMustBeGreaterThanZero);
        }

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Schedule>(errors);
        }

        var schedule = new Schedule(
            scheduleId, ecosystemId, relayId,
            cron!, durationMin,
            isFadeMode, isEnabled,
            createdAt: DateTime.UtcNow);

        return Result<Schedule>.Success(schedule);
    }

    public Result Update(
        string rawCronExpression,
        ICronValidator cronValidator,
        double durationMin,
        bool isFadeMode,
        bool isEnabled)
    {
        var errors = new List<string>();

        CronSchedule? cron = DomainGuard.Collect(
            CronSchedule.Create(rawCronExpression, cronValidator), errors);
        if (durationMin <= ControlConstants.MinDuration)
        {
            errors.Add(ControlValidationMessages.DurationMinMustBeGreaterThanZero);
        }

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation<Schedule>(string.Join(", ", errors)));
        }

        CronExpression = cron!;
        DurationMin = durationMin;
        IsFadeMode = isFadeMode;
        IsEnabled = isEnabled;

        IncrementVersion();

        return Result.Success();
    }

    public void SetIsActive(bool state)
    {
        if (IsEnabled == state)
        {
            return;
        }

        IsEnabled = state;

        IncrementVersion();
    }
}
