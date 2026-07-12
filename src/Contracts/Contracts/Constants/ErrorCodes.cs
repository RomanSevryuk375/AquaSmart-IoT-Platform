namespace Contracts.Constants;

public static class ErrorCodes
{
    public const string ValidationError = "Validation.Error";

    public static class Identity
    {
        public const string InvalidCredentials = "Auth.Invalid";
        public const string TokenReuse = "Security.Breach";
        public const string EmailBusy = "Email.Busy";
        public const string RegisterFailure = "Register.Failure";
        public const string ProfileUpdateFailed = "Profile.UpdateFailed";
        public const string PasswordChangeFailed = "Password.ChangeFailed";
    }

    public static class Hardware
    {
        public const string NotFound = "Hardware.NotFound";
        public const string Mismatch = "Hardware.Mismatch";
    }

    public static class Device
    {
        public const string NotFound = "Device.NotFound";
    }

    public static class Security
    {
        public const string AccessDenied = "Access.Denied";
    }

    public static class Grpc
    {
        public const string Conflict = "Grpc.Conflict";
        public const string Error = "Grpc.Error";
    }

    public static class Validation
    {
        public const string GuidInvalid = "Guid.Invalid";
    }

    public static class Outbox
    {
        public const string PublishError = "Outbox.PublishError";
    }

    public static class NotificationProvider
    {
        public const string TgProviderError = "TgProvider.Error";
        public const string EmailProviderError = "EmailProvider.Error";
        public const string TgChatIdMissing = "TgProvider.ChatIdMissing";
        public const string EmailMissing = "EmailProvider.EmailMissing";
        public const string UserDisabledOrNotFound = "Notification.UserDisabledOrNotFound";
        public const string NoActiveChannels = "Notification.NoActiveChannels";
    }
}

