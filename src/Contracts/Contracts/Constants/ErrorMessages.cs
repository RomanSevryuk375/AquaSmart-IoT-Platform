namespace Contracts.Constants;

public static class ErrorMessages
{
    public const string ControllerNotFound = "Controller {0} not found.";
    public const string RelayCommandNotFound = "RelayCommand {0} not found";
    public const string RelayNotFound = "Relay {0} not found";
    public const string SensorNotFound = "Sensor {0} not found";
    public const string InvalidCredentials = "Invalid credentials.";
    public const string InvalidCredentialsOrControllerNotFound = "Invalid credentials or controller not found.";
    public const string CommandUnavailable = "Command is unavailable or was expired.";
    public const string SensorAndRelaySameController = "Sensor and Relay must belong to the same controller";
    public const string RelayNotFoundOrAccessDenied = "Relay {0} not found or access denied.";
    public const string SensorNotFoundOrAccessDenied = "Sensor {0} not found or access denied.";
    public const string SensorNotBelongToController = "Sensor {0} does not belong to controller {1}.";
    public const string YouDontOwnThisController = "Forbidden: You don't own this controller";
    public const string YouAreNotOwnerOfController = "You are not the owner of this controller";
    public const string InvalidDeviceToken = "Invalid device token";
    public const string DatabaseError = "Job.DatabaseError";
    public const string ExternalError = "External.Error";
    public const string AccessDenied = "Access.Denied";
    public const string ControllerNotFoundPlain = "Controller not found";

    public static class Identity
    {
        public const string UserNotFound = "User not found.";
        public const string InvalidTokenFormat = "Invalid token format.";
        public const string TokenInvalidOrExpired = "Token is invalid or expired.";
        public const string TokenReuseDetected = "Token reuse detected. All sessions revoked.";
        public const string EmailInUse = "Email is already in use.";
        public const string PhoneRequired = "Phone number is required.";
        public const string PhoneFormatInvalid = "Phone number should be in format +375XXXXXXXXX or 80XXXXXXXXX.";
        public const string EmailRequired = "Email address is required.";
        public const string EmailTooLong = "Email address cannot exceed 256 characters.";
        public const string EmailFormatInvalid = "Invalid email address format.";
        public const string TimeZoneRequired = "Time zone is required.";
        public const string TimeZoneNotRecognized = "Time zone '{0}' is not recognized.";
        public const string PasswordMinLength = "Password must be at least 8 characters long.";
    }

    public static class Hardware
    {
        public const string ComponentsNotFound = "One or more hardware components were not found in DeviceService.";
        public const string MismatchFormat = "Sensor {0} and Relay {1} do not belong to the same controller.";
        public const string MismatchSimple = "Sensor and Relay do not belong to the same controller.";
        public const string NotFoundFormat = "Sensor {0} or Relay {1} does not exist.";
    }

    public static class Security
    {
        public const string YouAreNotOwnerOfEcosystem = "You are not the owner of this ecosystem";
        public const string YouAreNotOwnerOfReminder = "You are not the owner of this reminder.";
        public const string YouAreNotOwnerOfRule = "You are not the owner of this rule";
        public const string YouAreNotOwnerOfSchedule = "You are not the owner of this schedule";
        public const string YouAreNotOwnerOfVacationMode = "You are not the owner of this vacation mode";
        public const string ControllerInvalid = "controller invalid";
    }

    public static class Grpc
    {
        public const string CommunicationFailedFormat = "Communication failed: {0}";
    }

    public static class Validation
    {
        public const string GuidInvalidFormat = "UserId {0} or ControllerId {1} invalid Guid format";
    }

    public static class TelemetrySummary
    {
        public const string DataPointsCountMustBePositive = "Data points count must be greater than zero.";
        public const string MinValueCannotBeGreaterThanMaxValue = "MinValue cannot be greater than MaxValue.";
        public const string AvgValueMustBeBetweenMinAndMax = "AvgValue must be between MinValue and MaxValue.";
    }

    public static class Telemetry
    {
        public const string RecordedAtFuture = "recordedAt cannot be in the future.";
        public const string ExternalMessageIdEmpty = "externalMessageId must not be empty.";
        public const string SensorNotFoundFormat = "Sensor {0} not found";
    }

    public static class Sensor
    {
        public const string UnitMustNotBeEmpty = "Unit must not be empty.";
    }

    public static class MaintenanceLog
    {
        public const string ActionDateInFuture = "Action date cannot be in the future.";
    }

    public static class Reminder
    {
        public const string NotFoundFormat = "Reminder {0} not found.";
    }
}
