using Contracts.Abstractions;
using Contracts.Enums;

namespace Device.Domain.Entities;

public sealed class RelayEntity : IEntity
{
    private RelayEntity(
        Guid id,
        Guid controllerId,
        Guid? powerSensorId, 
        string name,
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        bool isNormalyOpen,
        RelayPurposeEnum purpose,
        bool isActive,
        bool isManual,
        DateTime createdAt)
    {
        Id = id;
        ControllerId = controllerId;
        PowerSensorId = powerSensorId;
        Name = name;
        ConnectionProtocol = connectionProtocol;
        ConnectionAddress = connectionAddress;
        IsNormalyOpen = isNormalyOpen;
        Purpose = purpose;
        IsActive = isActive;
        IsManual = isManual;
        CreatedAt = createdAt;
    }
    
    public Guid Id { get; private set; }
    public Guid ControllerId { get; private set; }
    public Guid? PowerSensorId { get; private set; }
    public string Name { get; private set; }
    public ConnectionProtocolEnum ConnectionProtocol { get; private set; }
    public string ConnectionAddress { get; private set; }
    public bool IsNormalyOpen { get; private set; }
    public RelayPurposeEnum Purpose { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsManual { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static (RelayEntity? relay, List<string>? errors) Create (
        Guid controllerId,
        Guid? powerSensorId,
        string name,
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        bool isNormalyOpen,
        RelayPurposeEnum purpose,
        bool isActive,
        bool isManual)
    {
        var errors = new List<string>();

        if (controllerId == Guid.Empty)
        {
            errors.Add("controllerId must not be empty.");
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
            return (null, errors);
        }

        var relay = new RelayEntity(
            Guid.NewGuid(),
            controllerId,
            powerSensorId,
            name.Trim(),
            connectionProtocol,
            connectionAddress.Trim(),
            isNormalyOpen,
            purpose,
            isActive,
            isManual,
            DateTime.UtcNow);

        return (relay, errors);
    }

    public List<string>? Update(
        Guid controllerId,
        ConnectionProtocolEnum connectionProtocol,
        string connectionAddress,
        RelayPurposeEnum purpose,
        bool isNormalyOpen)
    {
        var errors = new List<string>();

        if (controllerId == Guid.Empty)
        {
            errors.Add("controllerId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(connectionAddress))
        {
            errors.Add("connectionAddress must not be empty.");
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        ControllerId = controllerId;
        ConnectionProtocol = connectionProtocol;
        ConnectionAddress = connectionAddress.Trim();
        IsNormalyOpen = isNormalyOpen;
        Purpose = purpose;

        return null;
    }

    public void ToggleState()
    {
        IsActive = !IsActive;
    }

    public List<string>? SetName(string name)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name must not be empty.");
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        Name = name;

        return null;
    }

    public List<string>? SetPowerSensor(Guid powerSensorId)
    {
        var errors = new List<string>();

        if (powerSensorId == Guid.Empty)
        {
            errors.Add("powerSensorId must not be empty.");
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        PowerSensorId = powerSensorId;

        return null;
    }

    public void SetState(bool state)
    {
        if (IsActive == state)
        {
            return;
        }

        IsActive = state;
    }

    public void ToggleMode()
    {
        IsManual = !IsManual;
    }
}
