namespace Telemetry.TestShared.Builders;

public class SensorBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _controllerId = Guid.NewGuid();
    private Guid _ecosystemId = Guid.NewGuid();
    private string _name = "Test Sensor";
    private SensorType _type = SensorType.Temperature;
    private SensorState _state = SensorState.Active;
    private string _unit = "C";
    private double _lastValue = 25.0;
    private DateTime _updatedAt = DateTime.UtcNow;
    private DateTime _createdAt = DateTime.UtcNow;

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

    public SensorBuilder WithType(SensorType type)
    {
        _type = type;
        return this;
    }

    public SensorBuilder WithState(SensorState state)
    {
        _state = state;
        _overrideState = state;
        return this;
    }

    public SensorBuilder WithUnit(string unit)
    {
        _unit = unit;
        return this;
    }

    public SensorBuilder WithLastValue(double lastValue)
    {
        _lastValue = lastValue;
        return this;
    }

    public SensorBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
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
            _id, _controllerId, _ecosystemId,
            _name, _type, _state, _unit,
            _lastValue, _updatedAt, _createdAt);

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
