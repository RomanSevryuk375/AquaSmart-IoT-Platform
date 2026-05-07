using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Device.Domain.DomainEvents.RelayEvents;
using Device.Domain.Factories;

namespace Device.Domain.Entities;

public sealed class RelayEntity : AggregateRoot, IEntity
{
    private RelayEntity(
        Guid id,
        Guid controllerId,
        Guid userId,
        Guid? powerSensorId, 
        string name,
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        bool isNormallyOpen,
        RelayPurposeEnum purpose,
        bool isActive,
        bool isManual,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        PowerSensorId = powerSensorId;
        UserId = userId;
        Name = name;
        ConnectionProtocol = connectionProtocol;
        ConnectionAddress = connectionAddress;
        IsNormallyOpen = isNormallyOpen;
        Purpose = purpose;
        IsActive = isActive;
        IsManual = isManual;
        CreatedAt = createdAt;
    }
    
    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? PowerSensorId { get; private set; }
    public string Name { get; private set; }
    public ConnectionProtocolEnum ConnectionProtocol { get; private set; }
    public string ConnectionAddress { get; private set; }
    public bool IsNormallyOpen { get; private set; }
    public RelayPurposeEnum Purpose { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsManual { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<RelayEntity> Create (
        Guid controllerId,
        Guid userId,
        Guid? powerSensorId,
        string name,
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        bool isNormalyOpen,
        RelayPurposeEnum purpose,
        bool isActive,
        bool isManual)
    {
        var errors = new List<string>();

        if (controllerId == Guid.Empty)
        {
            errors.Add("controllerId must not be empty.");
        }

        if (userId == Guid.Empty)
        {
            errors.Add("userId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(connectionAddress))
        {
            errors.Add("connectionAddress must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result<RelayEntity>.Failure(
                Error.Validation(
                    "Relay.Invalid",
                    string.Join("; ", errors)));
        }

        var relay = new RelayEntity(
            Guid.NewGuid(),
            controllerId,
            userId,
            powerSensorId,
            name.Trim(),
            connectionProtocol,
            connectionAddress.Trim(),
            isNormalyOpen,
            purpose,
            isActive,
            isManual,
            DateTime.UtcNow);

        relay.RaiseEvent(new RelayCreatedDomainEvent
        {
            RelayId = relay.Id,
            ControllerId = relay.ControllerId,
            PowerSensorId = relay.PowerSensorId,
            Name = relay.Name,
            Purpose = relay.Purpose,
            IsManual = relay.IsManual,
            IsActive = relay.IsActive,
            CreatedAt = relay.CreatedAt
        });

        return Result<RelayEntity>.Success(relay);
    }

    public Result Update(
        Guid controllerId,
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        RelayPurposeEnum purpose,
        bool isNormalyOpen)
    {
        var errors = new List<string>();

        if (controllerId == Guid.Empty)
        {
            errors.Add("controllerId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(connectionAddress))
        {
            errors.Add("connectionAddress must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result.Failure(
                Error.Validation(
                    "Relay.Invalid",
                    string.Join("; ", errors)));
        }

        ControllerId = controllerId;
        ConnectionProtocol = connectionProtocol;
        ConnectionAddress = connectionAddress.Trim();
        IsNormallyOpen = isNormalyOpen;
        Purpose = purpose;

        RaiseEvent(new RelayUpdatedDomainEvent
        {
            RelayId = Id,
            ControllerId = ControllerId,
            PowerSensorId = PowerSensorId,
            Name = Name,
            Purpose = Purpose,
            IsManual = IsManual,
            IsActive = IsActive,
            CreatedAt = CreatedAt
        });

        return Result.Success();
    }

    public Result SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(
                Error.Validation(
                    "Command.Invalid",
                    "name must not be empty."));
        }

        Name = name;

        return Result.Success();
    }

    public Result SetPowerSensor(Guid powerSensorId)
    {
        if (powerSensorId == Guid.Empty)
        {
            return Result.Failure(
                Error.Validation(
                    "Command.Invalid",
                    "powerSensorId must not be empty."));
        }

        PowerSensorId = powerSensorId;

        RaiseEvent(new SetRelayPowerSensorDomainEvent
        {
            RelayId = Id,
            PowerSensorId = powerSensorId,
        });

        return Result.Success();
    }

    public void SetState(bool state)
    {
        if (IsActive == state)
        {
            return;
        }

        IsActive = state;

        RaiseEvent(new RelayStateChangedDomainEvent
        {
            ControllerId = ControllerId,
            RelayId = Id,
            Action = StateEvaluatorFactory.EvaluateBool(IsActive),
            ExpireAt = DateTime.UtcNow.AddMinutes(5)
        });
    }

    public void SetMode(bool mode)
    {
        if (IsManual == mode)
        {
            return;
        }

        IsManual = mode;

        RaiseEvent(new RelayModeChangedDomainEvent
        {
            RelayId = Id,
            IsManual = IsManual
        });
    }

    public void MarkAsDeleted()
    {
        RaiseEvent(new RelayDeletedDomainEvent
        {
            RelayId = Id,
        });
    }
}
