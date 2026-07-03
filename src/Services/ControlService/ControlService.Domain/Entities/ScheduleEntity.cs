using Contracts.Abstractions;
using Contracts.Results;

namespace Control.Domain.Entities;

public sealed class ScheduleEntity : IEntity
{
    private ScheduleEntity(
        Guid id,
        Guid ecosystemId,
        Guid relayId,
        string cronExpression,
        double durationMin,
        bool isFadeMode,
        bool isEnable,
        DateTime createdAt)
    {
        Id = id;
        EcosystemId = ecosystemId;
        RelayId = relayId;
        CronExpression = cronExpression;
        DurationMin = durationMin;
        IsFadeMode = isFadeMode;
        IsEnable = isEnable;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Guid RelayId { get; private set; }
    public string CronExpression { get; private set; } = string.Empty;
    public double DurationMin { get; private set; }
    public bool IsFadeMode { get; private set; }
    public bool IsEnable { get; private set; }
    public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618
    private ScheduleEntity() { }
#pragma warning restore CS8618

    public static Result<ScheduleEntity> Create(
        Guid ecosystemId,
        Guid relayId,
        string cronExpression,
        double durationMin,
        bool isFadeMode,
        bool isEnable)
    {
        var errors = new List<string>();

        if (ecosystemId == Guid.Empty)
        {
            errors.Add("ecosystemId must not be empty.");
        }

        if (relayId == Guid.Empty)
        {
            errors.Add("relayId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            errors.Add("cronExpression must not be empty.");
        }

        if (durationMin < 0)
        {
            errors.Add("durationMin must not be less then zero.");
        }

        if (cronExpression.Length < 5)
        {
            errors.Add("cronExpression seems too short.");
        }

        if (errors.Count > 0)
        {
            return Result<ScheduleEntity>.Failure(
                Error.Validation(
                    "Schedule.Invalid",
                    string.Join("; ", errors)));
        }

        var schedule = new ScheduleEntity(
            Guid.NewGuid(),
            ecosystemId,
            relayId,
            cronExpression,
            durationMin,
            isFadeMode,
            isEnable,
            DateTime.UtcNow);

        return Result<ScheduleEntity>.Success(schedule);
    }

    public Result Update(
        string cronExpression,
        double durationMin,
        bool isFadeMode,
        bool isEnable)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            errors.Add("cronExpression must not be empty.");
        }

        if (durationMin < 0)
        {
            errors.Add("durationMin must not be less then zero.");
        }

        if (cronExpression.Length < 5)
        {
            errors.Add("cronExpression seems too short.");
        }

        if (errors.Count > 0)
        {
            return Result.Failure(Error.Validation(
                "Schedule.Invalid",
                string.Join("; ", errors)));
        }

        CronExpression = cronExpression;
        DurationMin = durationMin;
        IsFadeMode = isFadeMode;
        IsEnable = isEnable;

        return Result.Success();
    }
}
