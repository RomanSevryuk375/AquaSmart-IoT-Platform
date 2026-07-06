namespace Control.Application.Interfaces;

public interface IRuleSensorBoundRequest
{
    public Guid RuleId { get; }
    public Guid SensorId { get; }
}
