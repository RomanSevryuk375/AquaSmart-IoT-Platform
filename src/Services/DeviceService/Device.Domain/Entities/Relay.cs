using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Domain.Events.RelayEvents;

namespace Device.Domain.Entities;

public sealed class Relay : AggregateRoot, IEntity
{
    private Relay(
        Guid id,
        Guid controllerId,
        Guid userId,
        Guid? powerSensorId,
        DeviceName deviceName,
        ConnectionAddress connectionAddress,
        bool isNormallyOpen,
        RelayPurpose purpose,
        bool isActive,
        bool isManual,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        PowerSensorId = powerSensorId;
        UserId = userId;
        Name = deviceName;
        ConnectionAddress = connectionAddress;
        IsNormallyOpen = isNormallyOpen;
        Purpose = purpose;
        IsActive = isActive;
        IsManual = isManual;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private Relay() { }
#pragma warning restore CS8618

    public Guid Id { get; init; }
    public Guid ControllerId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? PowerSensorId { get; private set; }
    public DeviceName Name { get; private set; }
    public ConnectionAddress ConnectionAddress { get; private set; }
    public bool IsNormallyOpen { get; private set; }
    public RelayPurpose Purpose { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsManual { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Relay> Create(
        Guid id,
        Guid controllerId,
        Guid userId,
        Guid? powerSensorId,
        string rawName,
        ConnectionProtocol connectionProtocol,
        string rawConnectionAddress,
        bool isNormallyOpen,
        RelayPurpose purpose,
        bool isActive,
        bool isManual)
    {
        var errors = new List<string>();
        Result<DeviceName> nameResult = DeviceName.Create(rawName);
        if (nameResult.IsFailure)
        {
            errors.Add(nameResult.Error.Message);
        }

        Result<ConnectionAddress> addressResult = ConnectionAddress.Create(
            connectionProtocol, rawConnectionAddress);
        if (addressResult.IsFailure)
        {
            errors.Add(addressResult.Error.Message);
        }

        if (errors.Count != 0)
        {
            return Result<Relay>.Failure(Error.Validation<Relay>(
                string.Join(", ", errors)));
        }

        var relay = new Relay(
            id, controllerId, userId, powerSensorId,
            nameResult.Value, addressResult.Value, isNormallyOpen, purpose,
            isActive, isManual,
            createdAt: DateTime.UtcNow);

        relay.RaiseEvent(new RelayCreatedDomainEvent
        {
            RelayId = relay.Id,
            ControllerId = relay.ControllerId,
            PowerSensorId = relay.PowerSensorId,
            Name = relay.Name.Value,
            Purpose = relay.Purpose,
            IsManual = relay.IsManual,
            IsActive = relay.IsActive,
            CreatedAt = relay.CreatedAt
        });

        return Result<Relay>.Success(relay);
    }

    public Result Update(
        Guid controllerId,
        Guid userId,
        ConnectionProtocol connectionProtocol,
        string rawConnectionAddress,
        RelayPurpose purpose,
        bool isNormallyOpen)
    {
        Result<ConnectionAddress> addressResult = ConnectionAddress.Create(
            connectionProtocol, rawConnectionAddress);
        if (addressResult.IsFailure)
        {
            return Result.Failure(addressResult.Error);
        }

        ControllerId = controllerId;
        ConnectionAddress = addressResult.Value;
        IsNormallyOpen = isNormallyOpen;
        Purpose = purpose;

        RaiseEvent(new RelayUpdatedDomainEvent
        {
            RelayId = Id,
            UserId = userId,
            ControllerId = ControllerId,
            PowerSensorId = PowerSensorId,
            Name = Name.Value,
            Purpose = Purpose,
            IsManual = IsManual,
            IsActive = IsActive,
            CreatedAt = CreatedAt
        });

        IncrementVersion();

        return Result.Success();
    }

    public Result SetName(
        Guid userId,
        string rawName)
    {
        Result<DeviceName> nameResult = DeviceName.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result<Relay>.Failure(nameResult.Error);
        }

        Name = nameResult.Value;

        RaiseEvent(new RelayUpdatedDomainEvent
        {
            RelayId = Id,
            UserId = userId,
            ControllerId = ControllerId,
            PowerSensorId = PowerSensorId,
            Name = Name.Value,
            Purpose = Purpose,
            IsManual = IsManual,
            IsActive = IsActive,
            CreatedAt = CreatedAt
        });

        IncrementVersion();

        return Result.Success();
    }

    public Result SetPowerSensor(Sensor sensor, Guid userId)
    {
        if (sensor is not VoltageSensor)
        {
            return Result.Failure(Error.Conflict<Relay>(
                    RelayErrors.InvalidPowerSensorType));
        }

        PowerSensorId = sensor.Id;

        RaiseEvent(new SetRelayPowerSensorDomainEvent
        {
            RelayId = Id,
            PowerSensorId = sensor.Id,
            UserId = userId,
        });

        IncrementVersion();

        return Result.Success();
    }

    public Result RemovePowerSensor(Sensor sensor, Guid userId)
    {
        if (PowerSensorId != sensor.Id)
        {
            return Result.Failure(Error.Conflict<Relay>(
                $"Specified sensor {sensor.Id} is not assigned " +
                $"as the power sensor for this relay {Id}."));
        }

        PowerSensorId = null;

        RaiseEvent(new RemoveRelayPowerSensorDomainEvent
        {
            RelayId = Id,
            PowerSensorId = sensor.Id,
            UserId = userId,
        });

        IncrementVersion();

        return Result.Success();
    }

    public void SetState(bool state, Guid userId)
    {
        if (IsActive == state)
        {
            return;
        }

        IsActive = state;

        RaiseEvent(new RelayStateChangedDomainEvent
        {
            UserId = userId,
            ControllerId = ControllerId,
            RelayId = Id,
            TargetState = IsActive,
            ExpireAt = DateTime.UtcNow.AddMinutes(RelayCommandConstants.DefaultExpiryMinutes)
        });

        IncrementVersion();
    }

    public void SetMode(bool mode, Guid userId)
    {
        if (IsManual == mode)
        {
            return;
        }

        IsManual = mode;

        RaiseEvent(new RelayModeChangedDomainEvent
        {
            UserId = userId,
            RelayId = Id,
            IsManual = IsManual
        });

        IncrementVersion();
    }

    public void MarkAsDeleted(Guid userId)
    {
        RaiseEvent(new RelayDeletedDomainEvent
        {
            RelayId = Id,
            UserId = userId
        });
    }
}
