using Control.TestShared.Constants;

namespace Control.TestShared.Builders;

public class SensorBuilder
{
    private Guid _id = ControlTestConstants.SensorId;
    private Guid _controllerId = ControlTestConstants.ControllerId;
    private Guid _ecosystemId = ControlTestConstants.EcosystemId;
    private string _name = "Test Sensor";
    private SensorState _state = SensorState.Active;
    private SensorType _type = SensorType.Temperature;
    private DateTime _createdAt = DateTime.UtcNow;

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

    public SensorBuilder WithEcosystemId(Guid ecosystemId)
    {
        _ecosystemId = ecosystemId;
        return this;
    }

    public SensorBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SensorBuilder WithState(SensorState state)
    {
        _state = state;
        return this;
    }

    public SensorBuilder WithType(SensorType type)
    {
        _type = type;
        return this;
    }

    public SensorBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Sensor Build()
    {
        Result<Sensor> result = Sensor.Create(
            _id,
            _controllerId,
            _ecosystemId,
            _name,
            _state,
            _type,
            _createdAt);

        if (result.IsFailure)
        {
            throw new ArgumentException($"SensorBuilder failed: {result.Error.Message}");
        }

        Sensor sensor = result.Value;
        sensor.ClearDomainEvents();
        return sensor;
    }
}
