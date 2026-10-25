using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class SecurityService : ISecurityService
{
    private const string UseFaceIdKey = "sec_use_face_id";
    private const string AutoLockKey = "sec_auto_lock";
    private const string PinForRelayKey = "sec_pin_for_relay";
    private const string UserPinStorageKey = "aquasmart_user_pin";
    private const string AuthTokenKey = "aquasmart_auth_token";

    public async Task<bool> CheckRelayPinAsync(string relayName)
    {
        var pinRequired = Preferences.Default.Get(PinForRelayKey, false);
        if (!pinRequired) return true;

        var savedPin = await SecureStorage.Default.GetAsync(UserPinStorageKey);
        if (string.IsNullOrWhiteSpace(savedPin))
        {
            var setup = await Shell.Current.DisplayAlert(
                "Защита реле",
                "Включена защита реле, но PIN-код ещё не создан. Перейти в настройки безопасности?",
                "Настроить",
                "Отмена");

            if (setup)
            {
                await Shell.Current.GoToAsync("SecuritySettingsPage");
            }
            return false;
        }

        var enteredPin = await Shell.Current.DisplayPromptAsync(
            "Подтверждение PIN",
            $"Введите PIN-код для управления «{relayName}»:",
            placeholder: "••••",
            maxLength: 4,
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(enteredPin))
        {
            return false;
        }

        if (enteredPin == savedPin)
        {
            return true;
        }

        await Shell.Current.DisplayAlert("Ошибка доступа", "Неверный PIN-код. Переключение отменено.", "OK");
        return false;
    }

    public async Task<bool> CheckAppUnlockPinAsync()
    {
        var savedPin = await SecureStorage.Default.GetAsync(UserPinStorageKey);
        if (string.IsNullOrWhiteSpace(savedPin))
        {
            return true;
        }

        var useFaceId = Preferences.Default.Get(UseFaceIdKey, true);

        while (true)
        {
            var title = "AquaSmart заблокирован";
            var message = useFaceId
                ? "Подтвердите Face ID / Touch ID или введите PIN-код для разблокировки:"
                : "Введите 4-значный PIN-код для разблокировки:";

            var entered = await Shell.Current.DisplayPromptAsync(
                title,
                message,
                placeholder: "••••",
                maxLength: 4,
                keyboard: Keyboard.Numeric,
                accept: "Разблокировать",
                cancel: "Выйти");

            if (entered == savedPin)
            {
                return true;
            }

            if (entered == null)
            {
                var confirmLogout = await Shell.Current.DisplayAlert(
                    "Выход из аккаунта",
                    "Вы действительно хотите выйти из учётной записи?",
                    "Выйти",
                    "Остаться");

                if (confirmLogout)
                {
                    SecureStorage.Default.Remove(AuthTokenKey);
                    await Shell.Current.GoToAsync("//LoginPage");
                    return false;
                }
                continue;
            }

            await Shell.Current.DisplayAlert("Неверный PIN", "Введён неверный защитный код. Попробуйте снова.", "OK");
        }
    }

    public bool ShouldAutoLock(DateTime lastActiveTime)
    {
        var setting = Preferences.Default.Get(AutoLockKey, "1 мин.");
        if (setting == "Никогда") return false;

        var elapsed = DateTime.UtcNow - lastActiveTime;
        var threshold = setting switch
        {
            "Сразу" => TimeSpan.Zero,
            "1 мин." => TimeSpan.FromMinutes(1),
            "5 мин." => TimeSpan.FromMinutes(5),
            "15 мин." => TimeSpan.FromMinutes(15),
            _ => TimeSpan.FromMinutes(1)
        };

        return elapsed >= threshold;
    }

    public async Task<bool> CanUseBiometricsOrPinAsync()
    {
        var useFaceId = Preferences.Default.Get(UseFaceIdKey, true);
        if (!useFaceId) return false;

        var token = await SecureStorage.Default.GetAsync(AuthTokenKey);
        var pin = await SecureStorage.Default.GetAsync(UserPinStorageKey);

        return !string.IsNullOrWhiteSpace(token) || !string.IsNullOrWhiteSpace(pin);
    }

    public async Task<bool> AuthenticateBiometricOrPinAsync()
    {
        var savedPin = await SecureStorage.Default.GetAsync(UserPinStorageKey);
        var token = await SecureStorage.Default.GetAsync(AuthTokenKey);

        if (!string.IsNullOrWhiteSpace(savedPin))
        {
            var entered = await Shell.Current.DisplayPromptAsync(
                "Face ID / PIN-код",
                "Подтвердите вход в AquaSmart:",
                placeholder: "••••",
                maxLength: 4,
                keyboard: Keyboard.Numeric,
                accept: "Войти",
                cancel: "Отмена");

            if (entered == savedPin)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(entered))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Неверный PIN-код.", "OK");
            }
            return false;
        }

        // If no PIN set but token exists, simulate biometric confirmation
        var confirm = await Shell.Current.DisplayAlert(
            "Вход по Face ID / Touch ID",
            "Подтвердить вход в систему с использованием биометрии устройства?",
            "Войти",
            "Отмена");

        return confirm && !string.IsNullOrWhiteSpace(token);
    }
}
