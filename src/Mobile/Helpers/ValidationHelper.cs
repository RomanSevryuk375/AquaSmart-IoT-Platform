using System.Text.RegularExpressions;
using Mobile.Services;

namespace Mobile.Helpers;

/// <summary>
/// Client-side validation helper aligned with backend FluentValidation rules and ASP.NET Core Identity.
/// Prevents sending invalid requests to the API, maps field-level errors and provides localized messages (RU/EN).
/// </summary>
public static partial class ValidationHelper
{
    // Backend: IdentityConstants.PhoneNumberRegex = @"^(\+375|80)(29|44|33|25)\d{7}$"
    [GeneratedRegex(@"^(\+375|80)(29|44|33|25)\d{7}$")]
    private static partial Regex PhoneRegex();

    // Backend: IdentityConstants.EmailNumberRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
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
        var isEn = LocalizationService.Instance.IsEnglish;
        if (string.IsNullOrWhiteSpace(name))
        {
            error = isEn ? "Please enter your name" : "Укажите ваше имя";
            return false;
        }

        var trimmed = name.Trim();
        if (trimmed.Length < 2)
        {
            error = isEn ? "Name must contain at least 2 characters" : "Имя должно содержать не менее 2 символов";
            return false;
        }

        if (trimmed.Length > MaxNameLength)
        {
            error = isEn ? $"Name cannot exceed {MaxNameLength} characters" : $"Имя не может превышать {MaxNameLength} символов";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateEmail(string? email, out string? error)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        if (string.IsNullOrWhiteSpace(email))
        {
            error = isEn ? "Please enter your email address" : "Укажите адрес электронной почты";
            return false;
        }

        var trimmed = email.Trim();
        if (trimmed.Length > MaxEmailLength)
        {
            error = isEn ? $"Email cannot exceed {MaxEmailLength} characters" : $"Email не может превышать {MaxEmailLength} символов";
            return false;
        }

        if (!EmailRegex().IsMatch(trimmed))
        {
            error = isEn ? "Invalid email format (e.g. user@example.com)" : "Некорректный формат почты (пример: user@example.com)";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidatePhone(string? phone, out string? error)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        if (string.IsNullOrWhiteSpace(phone))
        {
            error = isEn ? "Please enter your phone number" : "Укажите номер телефона";
            return false;
        }

        var clean = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        if (!PhoneRegex().IsMatch(clean))
        {
            error = isEn ? "Phone number must be in format +375XXXXXXXXX or 80XXXXXXXXX" : "Номер должен быть в формате +375XXXXXXXXX или 80XXXXXXXXX";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Validates password according to backend ASP.NET Core Identity policy:
    /// - RequiredLength >= 8
    /// - RequireDigit = true
    /// - RequireLowercase = true
    /// - RequireNonAlphanumeric = true
    /// </summary>
    public static bool ValidatePassword(string? password, out string? error)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        if (string.IsNullOrWhiteSpace(password))
        {
            error = isEn ? "Please enter password" : "Введите пароль";
            return false;
        }

        var missing = new List<string>();

        if (password.Length < MinPasswordLength)
        {
            missing.Add(isEn ? $"at least {MinPasswordLength} characters" : $"не менее {MinPasswordLength} символов");
        }

        if (!password.Any(ch => ch >= 'a' && ch <= 'z'))
        {
            missing.Add(isEn ? "a lowercase letter (a-z)" : "строчную латинскую букву (a-z)");
        }

        if (!password.Any(ch => ch >= '0' && ch <= '9'))
        {
            missing.Add(isEn ? "a digit (0-9)" : "цифру (0-9)");
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            missing.Add(isEn ? "a special character (!@#$%^&* etc.)" : "спецсимвол (!@#$%^&* и др.)");
        }

        if (missing.Count > 0)
        {
            error = isEn ? $"Password must contain: {string.Join(", ", missing)}" : $"Пароль должен содержать: {string.Join(", ", missing)}";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateConfirmPassword(string? password, string? confirmPassword, out string? error)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            error = isEn ? "Repeat password to confirm" : "Повторите пароль для подтверждения";
            return false;
        }

        if (password != confirmPassword)
        {
            error = isEn ? "Passwords do not match" : "Пароли не совпадают";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateMacAddress(string? mac, out string? error)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        if (string.IsNullOrWhiteSpace(mac))
        {
            error = isEn ? "MAC address cannot be empty" : "MAC-адрес не может быть пустым";
            return false;
        }

        var trimmed = mac.Trim();
        if (!MacRegex().IsMatch(trimmed))
        {
            error = isEn ? "Invalid MAC address format (e.g. 24:6F:28:B4:72:01)" : "Неверный формат MAC-адреса (пример: 24:6F:28:B4:72:01)";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ValidateAquariumVolume(string? volumeStr, out double volume, out string? error)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        if (!double.TryParse(volumeStr?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out volume) || volume <= 0)
        {
            error = isEn ? "Tank volume must be greater than 0" : "Объём аквариума должен быть числом больше 0";
            return false;
        }

        if (volume > 100000)
        {
            error = isEn ? "Volume is unrealistically large (max 100 000 L)" : "Указан нереалистично большой объём (максимум 100 000 л)";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Deconstructs and maps raw Identity/FluentValidation error strings into dedicated field-level errors.
    /// </summary>
    public static void MapServerRegistrationErrors(
        string? rawError,
        out string? fullNameError,
        out string? emailError,
        out string? phoneError,
        out string? passwordError,
        out string? generalError)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        fullNameError = null;
        emailError = null;
        phoneError = null;
        passwordError = null;
        generalError = null;

        if (string.IsNullOrWhiteSpace(rawError))
        {
            generalError = isEn
                ? "Server connection issue. Please try again later or check your network."
                : "Проблемы с сервером. Пожалуйста, попробуйте позже или проверьте подключение к сети.";
            return;
        }

        var parts = rawError.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var passwordIssues = new List<string>();

        foreach (var rawPart in parts)
        {
            var part = rawPart.Trim();
            var lower = part.ToLowerInvariant();

            if (lower.Contains("non alphanumeric"))
            {
                passwordIssues.Add(isEn ? "a special character (!@#$%^&* etc.)" : "спецсимвол (!@#$%^&* и др.)");
            }
            else if (lower.Contains("lowercase"))
            {
                passwordIssues.Add(isEn ? "a lowercase letter (a-z)" : "строчную латинскую букву (a-z)");
            }
            else if (lower.Contains("uppercase"))
            {
                passwordIssues.Add(isEn ? "an uppercase letter (A-Z)" : "заглавную латинскую букву (A-Z)");
            }
            else if (lower.Contains("digit"))
            {
                passwordIssues.Add(isEn ? "a digit (0-9)" : "цифру (0-9)");
            }
            else if (lower.Contains("password must be at least") || lower.Contains("passwordminlength") || lower.Contains("passwordtooshort"))
            {
                passwordIssues.Add(isEn ? "at least 8 characters" : "не менее 8 символов");
            }
            else if (lower.Contains("email") || lower.Contains("emailinuse") || lower.Contains("already taken") || lower.Contains("already in use") || lower.Contains("duplicateemail"))
            {
                emailError = isEn ? "A user with this email is already registered" : "Пользователь с такой почтой уже зарегистрирован";
            }
            else if (lower.Contains("phone") || lower.Contains("phoneformatinvalid"))
            {
                phoneError = isEn ? "Phone number must be in format +375XXXXXXXXX or 80XXXXXXXXX" : "Номер должен быть в формате +375XXXXXXXXX или 80XXXXXXXXX";
            }
            else if (lower.Contains("name") && (lower.Contains("invalid") || lower.Contains("required") || lower.Contains("taken")))
            {
                fullNameError = isEn ? "Invalid user name (up to 128 characters)" : "Некорректное имя пользователя (до 128 символов)";
            }
            else if (lower.Contains("500") || lower.Contains("internal server") || lower.Contains("database") || lower.Contains("failed to connect") || lower.Contains("connection refused") || lower.Contains("service unavailable") || lower.Contains("bad gateway") || lower.Contains("timeout"))
            {
                generalError = isEn
                    ? "Server connection issue. Please try again later or check your network."
                    : "Проблемы с сервером. Пожалуйста, попробуйте позже или проверьте подключение к сети.";
            }
            else
            {
                generalError = LocalizeServerError(part);
            }
        }

        if (passwordIssues.Count > 0)
        {
            passwordError = isEn
                ? $"Password must contain: {string.Join(", ", passwordIssues)}"
                : $"Пароль должен содержать: {string.Join(", ", passwordIssues)}";
        }

        // If no field validation error was detected and generalError is empty, clearly state server issue
        if (fullNameError == null && emailError == null && phoneError == null && passwordError == null && string.IsNullOrWhiteSpace(generalError))
        {
            generalError = isEn
                ? "Server connection issue. Please try again later or check your network."
                : "Проблемы с сервером. Пожалуйста, попробуйте позже или проверьте подключение к сети.";
        }
    }

    /// <summary>
    /// Translates raw English backend and FluentValidation error messages into user-friendly Russian or English.
    /// </summary>
    public static string LocalizeServerError(string? rawError)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        if (string.IsNullOrWhiteSpace(rawError))
        {
            return isEn
                ? "Failed to connect to server. Check your network connection."
                : "Не удалось связаться с сервером. Проверьте подключение к сети.";
        }

        if (rawError.Contains(';') || rawError.Contains('\n'))
        {
            var parts = rawError.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var translated = parts.Select(LocalizeSingleServerError).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
            return string.Join("\n", translated);
        }

        return LocalizeSingleServerError(rawError);
    }

    private static string LocalizeSingleServerError(string rawPart)
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        var lower = rawPart.ToLowerInvariant();

        if (lower.Contains("non alphanumeric"))
        {
            return isEn ? "Password must contain a special character (!@#$%^&* etc.)." : "Пароль должен содержать спецсимвол (!@#$%^&* и др.).";
        }

        if (lower.Contains("password must be at least") || lower.Contains("passwordminlength") || lower.Contains("passwordtooshort"))
        {
            return isEn ? "Password must contain at least 8 characters." : "Пароль должен содержать не менее 8 символов.";
        }

        if (lower.Contains("lowercase"))
        {
            return isEn ? "Password must contain a lowercase letter (a-z)." : "Пароль должен содержать строчную латинскую букву (a-z).";
        }

        if (lower.Contains("uppercase"))
        {
            return isEn ? "Password must contain an uppercase letter (A-Z)." : "Пароль должен содержать заглавную латинскую букву (A-Z).";
        }

        if (lower.Contains("digit"))
        {
            return isEn ? "Password must contain a digit (0-9)." : "Пароль должен содержать цифру (0-9).";
        }

        if (lower.Contains("phone number should be in format") || lower.Contains("phoneformatinvalid"))
        {
            return isEn ? "Phone number must be in format +375XXXXXXXXX or 80XXXXXXXXX." : "Номер телефона должен быть в формате +375XXXXXXXXX или 80XXXXXXXXX.";
        }

        if (lower.Contains("is not a valid email address") || lower.Contains("emailaddress") || lower.Contains("invalid email address format"))
        {
            return isEn ? "Invalid email address format." : "Некорректный адрес электронной почты.";
        }

        if (lower.Contains("must not be empty") || lower.Contains("cannot be empty") || lower.Contains("is required"))
        {
            return isEn ? "Required field is missing." : "Обязательное поле не заполнено.";
        }

        if (lower.Contains("invalid credentials") || lower.Contains("invalid email or password") || lower.Contains("wrong password"))
        {
            return isEn ? "Invalid email or password." : "Неверный логин или пароль.";
        }

        if (lower.Contains("already in use") || lower.Contains("already taken") || lower.Contains("duplicateemail"))
        {
            return isEn ? "A user with this email is already registered." : "Пользователь с такой почтой уже зарегистрирован.";
        }

        if (lower.Contains("already exists") || lower.Contains("conflict"))
        {
            return isEn ? "User data already exists in the system." : "Пользователь с такими данными уже зарегистрирован.";
        }

        if (lower.Contains("unauthorized"))
        {
            return isEn ? "Session expired or authentication required." : "Сессия истекла или требуется повторная авторизация.";
        }

        if (lower.Contains("forbidden"))
        {
            return isEn ? "Access to the requested resource is forbidden." : "Доступ к запрошенному ресурсу запрещён.";
        }

        if (lower.Contains("not found"))
        {
            return isEn ? "Requested data not found on server." : "Запрошенные данные не найдены на сервере.";
        }

        if (lower.Contains("invalid mac address format"))
        {
            return isEn ? "Invalid controller MAC address format." : "Неверный формат MAC-адреса контроллера.";
        }

        if (lower.Contains("volume must be strictly greater than zero"))
        {
            return isEn ? "Tank volume must be greater than 0." : "Объём аквариума должен быть больше 0.";
        }

        if (lower.Contains("connection refused") || lower.Contains("failed to connect") || lower.Contains("network") || lower.Contains("unable to resolve host"))
        {
            return isEn ? "Failed to connect to server. Check your network connection." : "Не удалось связаться с сервером. Проверьте подключение к интернету.";
        }

        return rawPart;
    }
}
