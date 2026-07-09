using Control.TestShared.Constants;

namespace Control.TestShared.Builders;

public class RelayBuilder
{
    private Guid _id = ControlTestConstants.RelayId;
    private Guid _ecosystemId = ControlTestConstants.EcosystemId;
    private Guid _controllerId = ControlTestConstants.ControllerId;
    private Guid? _powerSensorId = null;
    private string _name = "Test Relay";
    private RelayPurpose _purpose = RelayPurpose.Pump;
    private bool _isManual = true;
    private bool _isActive = false;
    private DateTime _createdAt = DateTime.UtcNow;

    public RelayBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RelayBuilder WithEcosystemId(Guid ecosystemId)
    {
        _ecosystemId = ecosystemId;
        return this;
    }

    public RelayBuilder WithControllerId(Guid controllerId)
    {
        _controllerId = controllerId;
        return this;
    }

    public RelayBuilder WithPowerSensorId(Guid? powerSensorId)
    {
        _powerSensorId = powerSensorId;
        return this;
    }

    public RelayBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public RelayBuilder WithPurpose(RelayPurpose purpose)
    {
        _purpose = purpose;
        return this;
    }

    public RelayBuilder WithIsManual(bool isManual)
    {
        _isManual = isManual;
        return this;
    }

    public RelayBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public RelayBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Relay Build()
    {
        Result<Relay> result = Relay.Create(
            _id,
            _ecosystemId,
            _controllerId,
            _powerSensorId,
            _name,
            _purpose,
            _isManual,
            _isActive,
            _createdAt);

        if (result.IsFailure)
        {
            throw new ArgumentException($"RelayBuilder failed: {result.Error.Message}");
        }

        Relay relay = result.Value;
        relay.ClearDomainEvents();
        return relay;
    }
}
