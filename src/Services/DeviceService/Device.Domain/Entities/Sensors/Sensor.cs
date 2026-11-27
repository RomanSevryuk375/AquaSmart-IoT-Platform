using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
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
        SensorType type,
        string unit,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        UserId = userId;
        Name = name;
        ConnectionAddress = connectionAddress;
        Type = type;
        Unit = unit;
        State = SensorState.NoData;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618 
    protected Sensor() { }
#pragma warning restore CS8618

    public Guid Id { get; init; }
    public Guid ControllerId { get; private set; }
    public Guid UserId { get; init; }
    public DeviceName Name { get; private set; } = null!;
    public ConnectionAddress ConnectionAddress { get; private set; } = null!;
    public SensorState State { get; private set; }
    public DateTime CreatedAt { get; init; }

    public SensorType Type { get; private set; }
    public string Unit { get; private set; }

    public Result Update(
        Guid controllerId,
        string rawName,
        ConnectionProtocol connectionProtocol,
        string rawConnectionAddress)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(controllerId, nameof(ControllerId), errors);
        DeviceName? name = DomainGuard.Collect(DeviceName.Create(rawName), errors);
        ConnectionAddress? address = DomainGuard.Collect(
            ConnectionAddress.Create(connectionProtocol, rawConnectionAddress), errors);

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation<Sensor>(string.Join(", ", errors)));
        }

        ControllerId = controllerId;
        Name = name!;
        ConnectionAddress = address!;

        RaiseEvent(new SensorUpdatedDomainEvent
        {
            SensorId = Id,
            UserId = UserId,
            ControllerId = ControllerId,
            Name = Name.Value,
            Type = Type,
            State = State,
            Unit = Unit,
            CreatedAt = CreatedAt
        });

        IncrementVersion();

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
            UserId = UserId,
            State = State
        });

        IncrementVersion();
    }

    internal void RaiseCreatedEvent()
    {
        RaiseEvent(new SensorCreatedDomainEvent
        {
            SensorId = Id,
            UserId = UserId,
            ControllerId = ControllerId,
            Name = Name.Value,
            Type = Type,
            State = State,
            Unit = Unit,
            CreatedAt = CreatedAt
        });

        IncrementVersion();
    }

    public void MarkAsDeleted()
    {
        RaiseEvent(new SensorDeletedDomainEvent
        {
            SensorId = Id,
             UserId = UserId
        });
    }
}
