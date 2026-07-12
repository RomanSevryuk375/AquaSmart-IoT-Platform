using Contracts.Abstractions;
using Contracts.Constants;
using Contracts.Enums;
using Contracts.Results;
using Telemetry.Domain.Events;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Domain.Entities;

public sealed class Sensor : AggregateRoot, IEntity
{
    private Sensor(
        Guid id, Guid controllerId, Guid ecosystemId,
        DeviceName name, SensorType type, SensorState state, string unit,
        double lastValue, DateTime updatedAt,
        DateTime createdAt,
        bool isDataDelayed)
    {
        Id = id;
        ControllerId = controllerId;
        EcosystemId = ecosystemId;
        Name = name;
        Type = type;
        State = state;
        Unit = unit;
        LastValue = lastValue;
        UpdatedAt = updatedAt;
        CreatedAt = createdAt;
        IsDataDelayed = isDataDelayed;
    }

#pragma warning disable CS8618 
    private Sensor() { }
#pragma warning restore CS8618 

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public DeviceName Name { get; private set; }
    public SensorType Type { get; private set; }
    public SensorState State { get; private set; }
    public string Unit { get; private set; }
    public double LastValue { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDataDelayed { get; private set; }

    public static Result<Sensor> Create(
        Guid id, Guid controllerId, Guid ecosystemId,
        string rawName, SensorType type, SensorState state, string unit,
        double lastValue, DateTime updatedAt,
        DateTime createdAt)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unit))
        {
            errors.Add(ErrorMessages.Sensor.UnitMustNotBeEmpty);
        }

        Result<DeviceName> nameResut = DeviceName.Create(rawName);
        if (nameResut.IsFailure)
        {
            errors.Add(nameResut.Error.Message);
        }


        if (errors.Count > 0)
        {
            return Result<Sensor>.Failure(Error.Validation<Sensor>(
                      string.Join("; ", errors)));
        }

        var sensor = new Sensor(
            id, controllerId, ecosystemId,
            nameResut.Value, type, state, unit.Trim(),
            lastValue, updatedAt,
            createdAt,
            isDataDelayed: false);

        return Result<Sensor>.Success(sensor);
    }

    public Result Update(
        Guid controllerId,
        SensorType type,
        string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            return Result.Failure(Error.Validation<Sensor>(
                ErrorMessages.Sensor.UnitMustNotBeEmpty));
        }

        ControllerId = controllerId;
        Type = type;
        Unit = unit.Trim();

        IncrementVersion();

        return Result.Success();
    }

    public Result SetName(string rawName)
    {
        Result<DeviceName> nameResut = DeviceName.Create(rawName);
        if (nameResut.IsFailure)
        {
            return Result.Failure(nameResut.Error);
        }

        Name = nameResut.Value;

        IncrementVersion();

        return Result.Success();
    }

    public void UpdateLastValue(double newValue)
    {
        UpdatedAt = DateTime.UtcNow;
        LastValue = newValue;

        if (IsDataDelayed || State == SensorState.NoData)
        {
            IsDataDelayed = false;
            State = SensorState.Active;
        }

        IncrementVersion();
    }

    public void SetState(SensorState newStatus)
    {
        if (State == newStatus)
        {
            return;
        }

        State = newStatus;

        IncrementVersion();
    }

    public void SetType(SensorType type)
    {
        if (Type == type)
        {
            return;
        }

        Type = type;

        IncrementVersion();
    }

    public void MarkAsNoData()
    {
        IsDataDelayed = true;
        State = SensorState.NoData;

        RaiseEvent(new SensorNoDataDomainEvent
        {
            SensorId = Id,
            State = State,
            LastSeenAt = UpdatedAt,
        });

        IncrementVersion();
    }
}
