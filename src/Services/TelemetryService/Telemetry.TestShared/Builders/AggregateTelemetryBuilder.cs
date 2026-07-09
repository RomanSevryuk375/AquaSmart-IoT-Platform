namespace Telemetry.TestShared.Builders;

public class AggregateTelemetryBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _sensorId = Guid.NewGuid();
    private DateTime _periodStart = DateTime.UtcNow.Date;
    private PeriodType _period = PeriodType.Hourly;
    private double _minValue = 10.0;
    private double _maxValue = 30.0;
    private double _avgValue = 20.0;
    private int _dataPointsCount = 5;

    public AggregateTelemetryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public AggregateTelemetryBuilder WithSensorId(Guid sensorId)
    {
        _sensorId = sensorId;
        return this;
    }

    public AggregateTelemetryBuilder WithPeriodStart(DateTime periodStart)
    {
        _periodStart = periodStart;
        return this;
    }

    public AggregateTelemetryBuilder WithPeriod(PeriodType period)
    {
        _period = period;
        return this;
    }

    public AggregateTelemetryBuilder WithValues(double minValue, double maxValue, double avgValue, int dataPointsCount)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _avgValue = avgValue;
        _dataPointsCount = dataPointsCount;
        return this;
    }

    public AggregateTelemetry Build()
    {
        Result<AggregateTelemetry> result = AggregateTelemetry.Create(
            _id, _sensorId, _periodStart, _period,
            _minValue, _maxValue, _avgValue, _dataPointsCount);

        if (result.IsFailure)
        {
            throw new ArgumentException($"AggregateTelemetryBuilder failed: {result.Error.Message}");
        }

        AggregateTelemetry aggregateTelemetry = result.Value;
        aggregateTelemetry.ClearDomainEvents();
        return aggregateTelemetry;
    }
}
