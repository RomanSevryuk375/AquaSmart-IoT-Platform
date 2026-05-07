using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Device.Domain.DomainEvents.SensorEvents;

namespace Device.Domain.Entities;

public sealed class SensorEntity : AggregateRoot, IEntity
{
    private SensorEntity(
        Guid id,
        Guid controllerId,
        Guid userId,
        string name,
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        SensorTypeEnum type,
        SensorStateEnum state,
        string unit,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        UserId = userId;
        Name = name;
        ConnectionProtocol = connectionProtocol;
        ConnectionAddress = connectionAddress;
        Type = type;
        State = state;
        Unit = unit;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public ConnectionProtocolEnum ConnectionProtocol { get; private set; }
    public string ConnectionAddress { get; private set; }
    public SensorTypeEnum Type { get; private set; }
    public SensorStateEnum State { get; private set; }
    public string Unit { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<SensorEntity> Create(
        Guid controllerId,
        Guid userId,
        string name,
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        SensorTypeEnum type,
        string unit)
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

        if (string.IsNullOrWhiteSpace(unit))
        {
            errors.Add("unit must not be empty.");
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
            return Result<SensorEntity>.Failure(
                Error.Validation(
                    "Command.Invalid",
                    string.Join("; ", errors)));
        }

        var sensor = new SensorEntity(
            Guid.NewGuid(),
            controllerId,
            userId,
            name.Trim(),
            connectionProtocol,
            connectionAddress.Trim(),
            type,
            SensorStateEnum.NoData,
            unit.Trim(),
            DateTime.UtcNow);

        sensor.RaiseEvent(new SensorCreatedDomainEvent
        {
            SensorId = sensor.Id,
            ControllerId = sensor.ControllerId,
            Name = sensor.Name,
            Type = sensor.Type,
            State = sensor.State,
            Unit = sensor.Unit,
            CreatedAt = sensor.CreatedAt,
        });

        return Result<SensorEntity>.Success(sensor);
    }

    public Result Update(
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        Guid controllerId,
        SensorTypeEnum type,
        string unit)
    {
        var errors = new List<string>();

        if (controllerId == Guid.Empty)
        {
            errors.Add("controllerId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            errors.Add("unit must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(connectionAddress))
        {
            errors.Add("connectionAddress must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result.Failure(Error.Validation(
                    "Command.Invalid",
                    string.Join("; ", errors)));
        }

        ConnectionProtocol = connectionProtocol;
        ConnectionAddress = connectionAddress.Trim();
        ControllerId = controllerId;
        Type = type;
        Unit = unit.Trim();

        RaiseEvent(new SensorUpdatedDomainEvent
        {
            SensorId = Id,
            ControllerId = ControllerId,
            Name = Name,
            Type = Type,
            State = State,
            Unit = Unit,
            CreatedAt = CreatedAt
        });

        return Result.Success();
    }

    public void SetState(SensorStateEnum state)
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

    public void MarkAsDeleted()
    {
        RaiseEvent(new SensorDeletedDomainEvent
        {
            SensorId = Id,
        });
    }
}