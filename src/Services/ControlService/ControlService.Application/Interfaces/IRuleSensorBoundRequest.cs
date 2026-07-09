namespace Control.Application.Interfaces;

public interface IRuleSensorBoundRequest : IRuleBoundRequest
{
    public Guid SensorId { get; }
}
