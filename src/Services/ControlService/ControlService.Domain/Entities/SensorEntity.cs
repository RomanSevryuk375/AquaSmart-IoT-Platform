using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;

namespace Control.Domain.Entities;

public sealed class SensorEntity : IEntity
{
    private SensorEntity(
        Guid id, 
        Guid controllerId,
        Guid ecosystemId, 
        string name,
        SensorStateEnum state,
        SensorTypeEnum type, 
        double lastValue,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        EcosystemId = ecosystemId;
        Name = name;
        State = state;
        Type = type;
        LastValue = lastValue;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public string Name { get; private set; }
    public SensorStateEnum State { get; private set; }
    public SensorTypeEnum Type { get; private set; }
    public double LastValue { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<SensorEntity> Create(
        Guid id,
        Guid controllerId,
        Guid ecosystemId,
        string name,
        SensorStateEnum state,
        SensorTypeEnum type,
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

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result<SensorEntity>.Failure(
                Error.Validation(
                    "Sensor.Invalid",
                    string.Join("; ", errors)));
        }

        var sensor = new SensorEntity(
            id,
            controllerId,
            ecosystemId,
            name,
            state,
            type,
            0.0,
            createdAt);

        return Result<SensorEntity>.Success(sensor);
    }

    public void SetState(SensorStateEnum state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
    }

    public void SetLastValue(double value)
    {
        LastValue = value;
    }

    public void SetType(SensorTypeEnum type)
    {
        if (Type == type)
        {
            return;
        }

        Type = type;
    }

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
                "Sensor.Invalid",
                string.Join("; ", errors)));
        }

        Name = name;

        return Result.Success();
    }
}
