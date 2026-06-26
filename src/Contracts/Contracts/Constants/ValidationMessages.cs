namespace Contracts.Constants;

public static class ValidationMessages
{
    public const string InvalidMacAddressFormat = "Invalid MAC address format.";
    public const string MacAddressRequired = "MAC address cannot be empty.";
    public const string ItemsListNull = "Items list cannot be null.";
    public const string TelemetryBatchEmpty = "Telemetry batch cannot be empty.";
    public const string MaxBatchSize = "Maximum batch size is 50 items.";
    public const string SensorIdEmpty = "SensorId must not be empty.";
    public const string SensorIdEmptyGuid = "SensorId cannot be an empty Guid.";
    public const string ExternalMessageIdEmpty = "ExternalMessageId must not be empty.";
    public const string ExternalMessageIdTooLong = "ExternalMessageId is too long (max 100).";
    public const string RecordedAtProvided = "RecordedAt must be provided.";
    public const string RecordedAtFuture = "RecordedAt can not be in the future.";
}
