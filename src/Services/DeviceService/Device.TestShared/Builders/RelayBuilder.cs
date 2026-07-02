namespace Device.TestShared.Builders;

public class RelayBuilder
{
    private Guid _id = TestConstants.RelayId;
    private Guid _controllerId = TestConstants.ControllerId;
    private Guid _userId = TestConstants.UserId;
    private Guid? _powerSensorId = null;
    private readonly string _name = "Test Relay";
    private readonly ConnectionProtocol _protocol = ConnectionProtocol.Digital;
    private readonly string _address = TestConstants.ValidDigitalAddress;
    private readonly bool _isNormallyOpen = true;
    private RelayPurpose _purpose = RelayPurpose.Pump;
    private bool _isActive = false;
    private bool _isManual = true;

    public RelayBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RelayBuilder WithControllerId(Guid controllerId)
    {
        _controllerId = controllerId;
        return this;
    }

    public RelayBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public RelayBuilder WithPowerSensorId(Guid? powerSensorId)
    {
        _powerSensorId = powerSensorId;
        return this;
    }

    public RelayBuilder WithPurpose(RelayPurpose purpose)
    {
        _purpose = purpose;
        return this;
    }

    public RelayBuilder AsActive(bool isActive = true)
    {
        _isActive = isActive;
        return this;
    }

    public RelayBuilder AsAuto()
    {
        _isManual = false;
        return this;
    }

    public Relay Build()
    {
        Result<Relay> result = Relay.Create(
            _id, _controllerId, _userId, _powerSensorId, _name,
            _protocol, _address, _isNormallyOpen, _purpose, _isActive, _isManual);

        if (result.IsFailure)
        {
            throw new ArgumentException($"RelayBuilder failed: {result.Error.Message}");
        }

        Relay relay = result.Value;
        relay.ClearDomainEvents();
        return relay;
    }
}
