using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Domain.Entities;

public class RelayEntity : IEntity
{
    private RelayEntity(
        Guid id,
        Guid ecosystemId,
        Guid controllerId,
        Guid powerSensorId,
        string name,
        RelayPurposeEnum purpose,
        bool isManual,
        bool isActive,
        DateTime createdAt)
    {
        Id = id;
        EcosystemId = ecosystemId;
        ControllerId = controllerId;
        PowerSensorId = powerSensorId;
        Name = name;
        Purpose = purpose;
        IsManual = isManual;
        IsActive = isActive;
        CreatedAt = createdAt;
    }
    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid EcosystemId { get; private set; }
    public Guid PowerSensorId { get; private set; }
    public string Name { get; private set; }
    public RelayPurposeEnum Purpose { get; private set; }
    public bool IsManual { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static (RelayEntity? relay, List<string> errors) Create(
        Guid id,
        Guid ecosystemId,
        Guid controllerId,
        Guid powerSensorId,
        string name,
        RelayPurposeEnum purpose,
        bool isManual,
        bool isActive,
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

        if (powerSensorId == Guid.Empty)
        {
            errors.Add("powerSensorId must not be empty.");
        }

        if (string.IsNullOrEmpty(name))
        {
            errors.Add("name must not be empty.");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var relay = new RelayEntity(
            id,
            ecosystemId,
            controllerId,
            powerSensorId,
            name,
            purpose,
            isManual,
            isActive,
            createdAt);

        return (relay, errors);
    }

    public void SetPurpose(RelayPurposeEnum purpose)
    {
        if (Purpose == purpose)
        {
            return;
        }

        Purpose = purpose;
    }

    public void SetMode(bool mode)
    {
        IsManual = mode;
    }

    public void SetState(bool state)
    {
        IsActive = state;
    }

    public void SetPowerSensorId(Guid powerSensorId)
    {
        PowerSensorId = powerSensorId;
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
