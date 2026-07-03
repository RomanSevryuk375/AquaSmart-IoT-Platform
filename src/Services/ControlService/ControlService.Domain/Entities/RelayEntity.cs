using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Control.Domain.Events;
using Control.Domain.Factories;

namespace Control.Domain.Entities;

public sealed class RelayEntity : AggregateRoot, IEntity
{
    private RelayEntity(
        Guid id,
        Guid ecosystemId,
        Guid controllerId,
        Guid? powerSensorId,
        string name,
        RelayPurpose purpose,
        bool isManual,
        bool isActive,
        DateTime createdAt)
    {
        Id = id;
        EcosystemId = ecosystemId;
        ControllerId = controllerId;
        PowerSensorId = powerSensorId;
        Name = name;
        Purpose = purpose;
        IsManual = isManual;
        IsActive = isActive;
        CreatedAt = createdAt;
    }
    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Guid? PowerSensorId { get; private set; }
    public string Name { get; private set; }
    public RelayPurpose Purpose { get; private set; }
    public bool IsManual { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<RelayEntity> Create(
        Guid id,
        Guid ecosystemId,
        Guid controllerId,
        Guid? powerSensorId,
        string name,
        RelayPurpose purpose,
        bool isManual,
        bool isActive,
        DateTime createdAt)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("id must not be empty.");
        }

        if (ecosystemId == Guid.Empty)
        {
            errors.Add("ecosystemId must not be empty.");
        }

        if (controllerId == Guid.Empty)
        {
            errors.Add("controllerId must not be empty.");
        }

        if (powerSensorId == Guid.Empty)
        {
            errors.Add("powerSensorId must not be empty.");
        }

        if (string.IsNullOrEmpty(name))
        {
            errors.Add("name must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result<RelayEntity>.Failure(
                Error.Validation(
                    "Relay.Invalid",
                    string.Join("; ", errors)));
        }

        var relay = new RelayEntity(
            id,
            ecosystemId,
            controllerId,
            powerSensorId,
            name,
            purpose,
            isManual,
            isActive,
            createdAt);

        return Result<RelayEntity>.Success(relay);
    }

    public void SetPurpose(RelayPurpose purpose)
    {
        if (Purpose == purpose)
        {
            return;
        }

        Purpose = purpose;
    }

    public void SetMode(bool mode)
    {
        IsManual = mode;

        RaiseEvent(new RelayModeChangedDomainEvent
        {
            RelayId = Id,
            IsManual = IsManual,
        });
    }

    public void SetState(bool state, DateTime expireAt)
    {
        IsActive = state;

        RaiseEvent(new RelayStateChangedDomainEvent
        {
            ControllerId = ControllerId,
            RelayId = Id,
            Action = StateEvaluatorFactory.EvaluateBool(state),
            ExpireAt = expireAt,
        });
    }

    public void SetPowerSensorId(Guid powerSensorId) => PowerSensorId = powerSensorId;

    public Result SetName(string name)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name must not be empty.");
        }

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation(
                "Relay.Invalid",
                string.Join("; ", errors)));
        }

        Name = name;

        return Result.Success();
    }
}
