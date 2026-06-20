using Device.Domain.DomainEvents.SensorEvents;
using Device.Domain.Events.SensorEvents;

namespace Device.Domain.Entities.Sensors;

public abstract class Sensor : AggregateRoot, IEntity
{
    protected Sensor(
        Guid id,
        Guid controllerId,
        Guid userId,
        DeviceName name,
        ConnectionAddress connectionAddress,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        UserId = userId;
        Name = name;
        ConnectionAddress = connectionAddress;
        State = SensorState.NoData;
        CreatedAt = createdAt;
    }

    protected Sensor() { }

    public Guid Id { get; init; }
    public Guid ControllerId { get; private set; }
    public Guid UserId { get; init; }
    public DeviceName Name { get; private set; } = null!;
    public ConnectionAddress ConnectionAddress { get; private set; } = null!;
    public SensorState State { get; private set; }
    public DateTime CreatedAt { get; init; }

    public abstract SensorType Type { get; }
    public abstract string Unit { get; }

    public Result Update(
        Guid controllerId,
        string rawName,
        ConnectionProtocol connectionProtocol,
        string rawConnectionAddress)
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
            return Result.Failure(Error.Validation<Sensor>(
                string.Join(", ", errors)));
        }

        ControllerId = controllerId;
        Name = nameResut.Value;
        ConnectionAddress = addressResult.Value;

        RaiseEvent(new SensorUpdatedDomainEvent
        {
            SensorId = Id,
            ControllerId = ControllerId,
            Name = Name.Value,
            Type = Type,
            State = State,
            Unit = Unit,
            CreatedAt = CreatedAt
        });

        return Result.Success();
    }

    public void SetState(SensorState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        RaiseEvent(new SensorStateChangedDomainEvent
        {
            SensorId = Id,
            State = State
        });
    }

    internal void RaiseCreatedEvent()
    {
        RaiseEvent(new SensorCreatedDomainEvent
        {
            SensorId = Id,
            ControllerId = ControllerId,
            Name = Name.Value, 
            Type = Type,
            State = State,
            Unit = Unit,
            CreatedAt = CreatedAt
        });
    }

    public void MarkAsDeleted() =>
        RaiseEvent(new SensorDeletedDomainEvent { SensorId = Id });
}
