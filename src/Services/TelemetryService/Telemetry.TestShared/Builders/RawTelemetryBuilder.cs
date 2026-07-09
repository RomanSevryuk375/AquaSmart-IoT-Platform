namespace Telemetry.TestShared.Builders;

public class RawTelemetryBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _sensorId = Guid.NewGuid();
    private double _value = 23.4;
    private string _externalMessageId = "msg_123456";
    private DateTime _recordedAt = DateTime.UtcNow;

    public RawTelemetryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RawTelemetryBuilder WithSensorId(Guid sensorId)
    {
        _sensorId = sensorId;
        return this;
    }

    public RawTelemetryBuilder WithValue(double value)
    {
        _value = value;
        return this;
    }

    public RawTelemetryBuilder WithExternalMessageId(string externalMessageId)
    {
        _externalMessageId = externalMessageId;
        return this;
    }

    public RawTelemetryBuilder WithRecordedAt(DateTime recordedAt)
    {
        _recordedAt = recordedAt;
        return this;
    }

    public RawTelemetry Build()
    {
        Result<RawTelemetry> result = RawTelemetry.Create(
            _id, _sensorId, _value, _externalMessageId, _recordedAt);

        if (result.IsFailure)
        {
            throw new ArgumentException($"RawTelemetryBuilder failed: {result.Error.Message}");
        }

        RawTelemetry rawTelemetry = result.Value;
        rawTelemetry.ClearDomainEvents();
        return rawTelemetry;
    }
}
