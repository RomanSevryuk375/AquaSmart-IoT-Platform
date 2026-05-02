using Contracts.Abstractions;
using Contracts.Enums;

namespace Device.Domain.Entities;

public sealed class SensorEntity : IEntity
{
    private SensorEntity(
        Guid id,
        Guid controllerId,
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
    public string Name { get; private set; }
    public ConnectionProtocolEnum ConnectionProtocol { get; private set; }
    public string ConnectionAddress { get; private set; }
    public SensorTypeEnum Type { get; private set; }
    public SensorStateEnum State { get; private set; }
    public string Unit { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static (SensorEntity? sensor, List<string> errors) Create(
        Guid controllerId,
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
            return (null, errors);
        }

        var sensor = new SensorEntity(
            Guid.NewGuid(),
            controllerId,
            name.Trim(),
            connectionProtocol,
            connectionAddress.Trim(),
            type,
            SensorStateEnum.NoData,
            unit.Trim(),
            DateTime.UtcNow);

        return (sensor, errors);
    }

    public List<string>? Update(
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
            return errors;
        }

        ConnectionProtocol = connectionProtocol;
        ConnectionAddress = connectionAddress.Trim();
        ControllerId = controllerId;
        Type = type;
        Unit = unit.Trim();

        return null;
    }

    public void SetState(SensorStateEnum state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
    }
}