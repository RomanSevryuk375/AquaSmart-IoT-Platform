using System.Text.RegularExpressions;

namespace Mobile.Helpers;

/// <summary>
/// Client-side validation helper aligned with backend FluentValidation rules and constants.
/// Prevents sending invalid requests to the API and provides localized Russian messages.
/// </summary>
public static partial class ValidationHelper
{
    // Backend: IdentityConstants.PhoneNumberRegex = @"^(\+375|80)(29|44|33|25)\d{7}$"
    [GeneratedRegex(@"^(\+375|80)(29|44|33|25)\d{7}$")]
    private static partial Regex PhoneRegex();

    // Backend: IdentityConstants.EmailNumberRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    // Backend: ControllerConstants.MacAddressRegex = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$"
    [GeneratedRegex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$")]
    private static partial Regex MacRegex();

    public const int MinPasswordLength = 8;
    public const int MaxEmailLength = 256;
    public const int MaxNameLength = 128;

    public static bool ValidateFullName(string? name, out string? error)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Укажите ваше имя";
            return false;
        }

        var trimmed = name.Trim();
        if (trimmed.Length < 2)
        {
            error = "Имя должно содержать не менее 2 символов";
            return false;
        }

        if (trimmed.Length > MaxNameLength)
        {
            error = $"Имя не может превышать {MaxNameLength} символов";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateEmail(string? email, out string? error)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            error = "Укажите адрес электронной почты";
            return false;
        }

        var trimmed = email.Trim();
        if (trimmed.Length > MaxEmailLength)
        {
            error = $"Email не может превышать {MaxEmailLength} символов";
            return false;
        }

        if (!EmailRegex().IsMatch(trimmed))
        {
            error = "Некорректный формат почты (пример: user@example.com)";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidatePhone(string? phone, out string? error)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            error = "Укажите номер телефона";
            return false;
        }

        // Remove spaces, hyphens, parentheses
        var clean = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        if (!PhoneRegex().IsMatch(clean))
        {
            error = "Номер телефона должен быть в формате +375XXXXXXXXX или 80XXXXXXXXX";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidatePassword(string? password, out string? error)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            error = "Введите пароль";
            return false;
        }

        if (password.Length < MinPasswordLength)
        {
            error = $"Пароль должен содержать не менее {MinPasswordLength} символов";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateConfirmPassword(string? password, string? confirmPassword, out string? error)
    {
        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            error = "Повторите пароль для подтверждения";
            return false;
        }

        if (password != confirmPassword)
        {
            error = "Пароли не совпадают";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateMacAddress(string? mac, out string? error)
    {
        if (string.IsNullOrWhiteSpace(mac))
        {
            error = "MAC-адрес не может быть пустым";
            return false;
        }

        var trimmed = mac.Trim();
        if (!MacRegex().IsMatch(trimmed))
        {
            error = "Неверный формат MAC-адреса (пример: 24:6F:28:B4:72:01)";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateAquariumVolume(string? volumeStr, out double volume, out string? error)
    {
        if (!double.TryParse(volumeStr?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out volume) || volume <= 0)
        {
            error = "Объём аквариума должен быть числом больше 0";
            return false;
        }

        if (volume > 100000)
        {
            error = "Указан нереалистично большой объём (максимум 100 000 л)";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Translates raw English backend and FluentValidation error messages into user-friendly Russian.
    /// </summary>
    public static string LocalizeServerError(string? rawError)
    {
        if (string.IsNullOrWhiteSpace(rawError))
        {
            return "Произошла непредвиденная ошибка на сервере";
        }

        var lower = rawError.ToLowerInvariant();

        if (lower.Contains("password must be at least") || lower.Contains("passwordminlength"))
        {
            return "Пароль должен содержать не менее 8 символов.";
        }

        if (lower.Contains("phone number should be in format") || lower.Contains("phoneformatinvalid"))
        {
            return "Номер телефона должен быть в формате +375XXXXXXXXX или 80XXXXXXXXX.";
        }

        if (lower.Contains("is not a valid email address") || lower.Contains("emailaddress"))
        {
            return "Некорректный адрес электронной почты.";
        }

        if (lower.Contains("must not be empty") || lower.Contains("cannot be empty"))
        {
            return "Обязательное поле не заполнено.";
        }

        if (lower.Contains("invalid credentials") || lower.Contains("invalid email or password") || lower.Contains("wrong password"))
        {
            return "Неверный логин или пароль.";
        }

        if (lower.Contains("already exists") || lower.Contains("conflict"))
        {
            return "Пользователь с такой почтой или телефоном уже зарегистрирован.";
        }

        if (lower.Contains("unauthorized"))
        {
            return "Сессия истекла или требуется повторная авторизация.";
        }

        if (lower.Contains("forbidden"))
        {
            return "Доступ к запрошенному ресурсу запрещён.";
        }

        if (lower.Contains("not found"))
        {
            return "Запрошенные данные не найдены на сервере.";
        }

        if (lower.Contains("invalid mac address format"))
        {
            return "Неверный формат MAC-адреса контроллера.";
        }

        if (lower.Contains("volume must be strictly greater than zero"))
        {
            return "Объём аквариума должен быть больше 0.";
        }

        return rawError;
    }
}
