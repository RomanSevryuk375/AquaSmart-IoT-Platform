namespace Contracts.Constants;

public static class ControlValidationMessages
{
    public const string HysteresisCannotBeNegative = "Hysteresis cannot be negative.";
    public const string CronExpressionCannotBeEmpty = "Cron expression cannot be empty.";
    public const string InvalidCronExpressionFormat = "Invalid cron expression: {0}";
    public const string StartDateMustBeBeforeEndDate = "StartDate must be strictly before EndDate.";
    public const string NameCannotBeEmpty = "Name cannot be empty.";
    public const string NameCannotExceedLimitFormat = "Name cannot exceed {0} characters.";
    public const string VolumeMustBeGreaterThanZero = "Volume must be strictly greater than zero.";
    public const string VolumeIsUnrealisticallyLarge = "Volume is unrealistically large.";
    public const string ConditionForSensorAlreadyExistsFormat = "A condition for Sensor {0} already exists.";
    public const string ConditionNotFound = "Condition not found.";
    public const string DurationMinMustBeGreaterThanZero = "DurationMin must be strictly greater than zero.";
    public const string CalculatedFeedCannotBeNegative = "CalculatedFeed cannot be negative.";
}
