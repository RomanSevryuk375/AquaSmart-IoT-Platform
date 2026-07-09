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
}
