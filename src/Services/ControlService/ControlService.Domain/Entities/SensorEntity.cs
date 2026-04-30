using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Domain.Entities;

public class SensorEntity : IEntity
{
    private SensorEntity(
        Guid id, 
        Guid controllerId,
        Guid ecosystemId, 
        string name,
        SensorStateEnum state,
        SensorTypeEnum type, 
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        EcosystemId = ecosystemId;
        Name = name;
        State = state;
        Type = type;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public string Name { get; private set; }
    public SensorStateEnum State { get; private set; }
    public SensorTypeEnum Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static (SensorEntity? sensor, List<string> errors) Create(
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
            return (null, errors);
        }

        var sensor = new SensorEntity(
            id,
            controllerId,
            ecosystemId,
            name,
            state,
            type,
            createdAt);

        return (sensor, errors);
    }

    public void SetState(SensorStateEnum state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
    }

    public void SetType(SensorTypeEnum type)
    {
        if (Type == type)
        {
            return;
        }

        Type = type;
    }

    public List<string>? SetName(string name)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name must not be empty.");
        }

        if (errors.Count != 0)
        {
            return errors;
        }

        Name = name;

        return null;
    }
}
