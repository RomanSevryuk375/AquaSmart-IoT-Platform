using Contracts.Abstractions;
using Contracts.Enums;

namespace Telemetry.Domain.Entities;

public sealed class SensorEntity : IEntity
{
    private SensorEntity(
        Guid id,
        Guid controllerId,
        Guid ecosystemId,
        string name,
        SensorTypeEnum type,
        SensorStateEnum state,
        string unit,
        double lastValue,
        DateTime updatedAt,
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

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public string Name { get; private set; }
    public SensorTypeEnum Type { get; private set; }
    public SensorStateEnum State { get; private set; }
    public string Unit { get; private set; }
    public double LastValue { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDataDelayed { get; private set; }

    public static (SensorEntity? sensor, List<string> errors) Create(
        Guid id,
        Guid controllerId,
        Guid ecosystemId,
        string name,
        SensorTypeEnum type,
        SensorStateEnum state,
        string unit,
        double lastValue,
        DateTime updatedAt,
        DateTime createdAt)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("Id must not be empty.");
        }

        if (controllerId == Guid.Empty)
        {
            errors.Add("Controller id must not be empty.");
        }

        if (ecosystemId == Guid.Empty)
        {
            errors.Add("Ecosystem id must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            errors.Add("Unit must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Name must not be empty.");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var sensor = new SensorEntity(
            id,
            controllerId,
            ecosystemId,
            name.Trim(),
            type,
            state,
            unit.Trim(),
            lastValue,
            updatedAt,
            createdAt,
            false);

        return (sensor, errors);
    }

    public List<string>? Update(
        Guid controllerId,
        SensorTypeEnum type,
        string unit)
    {
        var errors = new List<string>();

        if (controllerId == Guid.Empty)
        {
            errors.Add("Controller id must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            errors.Add("Unit must not be empty.");
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        ControllerId = controllerId;
        Type = type;
        Unit = unit.Trim();

        return null;
    }

    public List<string>? Rename(string name)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Name must not be empty.");
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Name = name;

        return null;
    }

    public void UpdateLastValue(double newValue)
    {
        UpdatedAt = DateTime.UtcNow;
        LastValue = newValue;

        if (IsDataDelayed || State == SensorStateEnum.NoData) 
        {
            IsDataDelayed = false;
            State = SensorStateEnum.Active;
        }
    }

    public void SetState(SensorStateEnum newStatus)
    {
        if (State == newStatus)
        {
            return;
        }

        State = newStatus;
    }

    public void SetDataDelayed(bool status)
    {
        IsDataDelayed = status;
    }
}
