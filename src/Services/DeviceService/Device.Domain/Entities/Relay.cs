using Contracts.Constants;
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
    public Relay() { }
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
        Result<DeviceName> nameResut = DeviceName.Create(rawName);
        if (nameResut.IsFailure)
        {
            errors.Add(nameResut.Error.Message);
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
            id,
            controllerId,
            userId,
            powerSensorId,
            nameResut.Value,
            addressResult.Value,
            isNormallyOpen,
            purpose,
            isActive,
            isManual,
            DateTime.UtcNow);

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
            ControllerId = ControllerId,
            PowerSensorId = PowerSensorId,
            Name = Name.Value,
            Purpose = Purpose,
            IsManual = IsManual,
            IsActive = IsActive,
            CreatedAt = CreatedAt
        });

        return Result.Success();
    }

    public Result SetName(string rawName)
    {
        Result<DeviceName> nameResut = DeviceName.Create(rawName);
        if (nameResut.IsFailure)
        {
            return Result<Relay>.Failure(nameResut.Error);
        }

        Name = nameResut.Value;

        return Result.Success();
    }

    public Result SetPowerSensor(Sensor sensor)
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
            TargetState = IsActive,
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

    public void MarkAsDeleted() => RaiseEvent(new RelayDeletedDomainEvent { RelayId = Id, });
}
