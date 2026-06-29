using Contracts.Enums;
using Contracts.Results;
using Device.Domain.Entities.Sensors;
using Device.Domain.Factories;
using Device.TestShared.Constants;

namespace Device.TestShared.Builders;

public class SensorBuilder
{
    private Guid _id = TestConstants.SensorId;
    private Guid _controllerId = TestConstants.ControllerId;
    private Guid _userId = TestConstants.UserId;
    private readonly string _name = "Test Sensor";
    private ConnectionProtocol _protocol = ConnectionProtocol.I2C;
    private string _address = TestConstants.ValidI2cAddress;
    private SensorType _type = SensorType.Temperature;
    private SensorState? _overrideState = null;

    public SensorBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SensorBuilder WithControllerId(Guid controllerId)
    {
        _controllerId = controllerId;
        return this;
    }

    public SensorBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public SensorBuilder WithType(SensorType type)
    {
        _type = type;
        return this;
    }

    public SensorBuilder WithAddress(ConnectionProtocol protocol, string address)
    {
        _protocol = protocol;
        _address = address;
        return this;
    }

    public SensorBuilder WithState(SensorState state)
    {
        _overrideState = state;
        return this;
    }

    public Sensor Build()
    {
        Result<Sensor> result = SensorFactory.CreateSensor(
            _id, _controllerId, _userId, _name, _protocol, _address, _type);

        if (result.IsFailure)
        {
            throw new ArgumentException($"SensorBuilder failed: {result.Error.Message}");
        }

        Sensor sensor = result.Value;

        if (_overrideState.HasValue)
        {
            sensor.SetState(_overrideState.Value);
        }

        sensor.ClearDomainEvents();
        return sensor;
    }
}
