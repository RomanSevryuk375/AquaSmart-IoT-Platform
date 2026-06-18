using Contracts.Abstractions;
using Contracts.Enums;
using Device.Domain.ValueObjects;
using Device.Domain.DomainEvents.SensorEvents;

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

    public void SetState(SensorState state)
    {
        if (State == state) return;

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
