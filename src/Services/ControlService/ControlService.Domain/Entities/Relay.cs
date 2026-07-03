using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Control.Domain.Events;
using Control.Domain.ValueObjects;

namespace Control.Domain.Entities;

public sealed class Relay : AggregateRoot, IEntity
{
    private Relay(
        Guid id,
        Guid ecosystemId,
        Guid controllerId,
        Guid? powerSensorId,
        Name name,
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

#pragma warning disable CS8618
    private Relay() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Guid? PowerSensorId { get; private set; }
    public Name Name { get; private set; }
    public RelayPurpose Purpose { get; private set; }
    public bool IsManual { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Relay> Create(
        Guid relayId,
        Guid ecosystemId,
        Guid controllerId,
        Guid? powerSensorId,
        string rawName,
        RelayPurpose purpose,
        bool isManual,
        bool isActive,
        DateTime createdAt)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result<Relay>.Failure(nameResult.Error);
        }

        var relay = new Relay(
            relayId, ecosystemId, controllerId, powerSensorId,
            nameResult.Value, purpose, isManual, isActive,
            createdAt);

        return Result<Relay>.Success(relay);
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
            TargetState = IsActive,
            ExpireAt = expireAt,
        });
    }

    public void SetPowerSensorId(Guid powerSensorId) => PowerSensorId = powerSensorId;

    public Result SetName(string rawName)
    {
        Result<Name> nameResult = Name.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result<Relay>.Failure(nameResult.Error);
        }

        Name = nameResult.Value;

        return Result.Success();
    }
}
