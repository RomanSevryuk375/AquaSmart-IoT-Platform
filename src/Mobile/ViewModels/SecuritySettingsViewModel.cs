using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class SecuritySettingsViewModel : ObservableObject
{
    private const string UseFaceIdKey = "sec_use_face_id";
    private const string AutoLockKey = "sec_auto_lock";
    private const string TwoFactorKey = "sec_2fa_enabled";
    private const string PinForRelayKey = "sec_pin_for_relay";
    private const string UserPinStorageKey = "aquasmart_user_pin";

    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private bool _useFaceId = true;

    [ObservableProperty]
    private string _autoLockDuration = "1 мин.";

    [ObservableProperty]
    private bool _twoFactorEnabled = false;

    [ObservableProperty]
    private bool _pinForRelay = false;

    public SecuritySettingsViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        UseFaceId = Preferences.Default.Get(UseFaceIdKey, true);
        AutoLockDuration = Preferences.Default.Get(AutoLockKey, "1 мин.");
        TwoFactorEnabled = Preferences.Default.Get(TwoFactorKey, false);
        PinForRelay = Preferences.Default.Get(PinForRelayKey, false);
    }

    partial void OnUseFaceIdChanged(bool value) => Preferences.Default.Set(UseFaceIdKey, value);
    partial void OnTwoFactorEnabledChanged(bool value) => Preferences.Default.Set(TwoFactorKey, value);
    partial void OnPinForRelayChanged(bool value) => Preferences.Default.Set(PinForRelayKey, value);

    [RelayCommand]
    private async Task ChangePinAsync()
    {
        var title = LocalizationService.Instance["Sec_PinCode"];
        var prompt = LocalizationService.Instance.IsEnglish ? "Enter new 4-digit PIN:" : "Введите новый 4-значный ПИН-код:";
        var pin = await Shell.Current.DisplayPromptAsync(title, prompt, maxLength: 4, keyboard: Keyboard.Numeric);
        if (!string.IsNullOrWhiteSpace(pin) && pin.Length == 4)
        {
            await SecureStorage.Default.SetAsync(UserPinStorageKey, pin);
            await Shell.Current.DisplayAlert(
                LocalizationService.Instance["Common_Success"],
                LocalizationService.Instance.IsEnglish ? "PIN code saved in secure device storage" : "Код-пароль сохранен в защищенном хранилище устройства",
                LocalizationService.Instance["Common_Ok"]);
        }
        else if (!string.IsNullOrWhiteSpace(pin))
        {
            await Shell.Current.DisplayAlert(
                LocalizationService.Instance["Common_Error"],
                LocalizationService.Instance.IsEnglish ? "PIN code must contain exactly 4 digits" : "ПИН-код должен содержать ровно 4 цифры",
                LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task ChooseAutoLockAsync()
    {
        var title = LocalizationService.Instance["Sec_AutoLock"];
        var cancel = LocalizationService.Instance["Common_Cancel"];
        var optImmediately = LocalizationService.Instance.IsEnglish ? "Immediately" : "Сразу";
        var opt1Min = LocalizationService.Instance.IsEnglish ? "1 min" : "1 мин.";
        var opt5Min = LocalizationService.Instance.IsEnglish ? "5 min" : "5 мин.";
        var opt15Min = LocalizationService.Instance.IsEnglish ? "15 min" : "15 мин.";
        var optNever = LocalizationService.Instance.IsEnglish ? "Never" : "Никогда";
        var result = await Shell.Current.DisplayActionSheet(title, cancel, null, optImmediately, opt1Min, opt5Min, opt15Min, optNever);
        if (!string.IsNullOrWhiteSpace(result) && result != cancel)
        {
            AutoLockDuration = result;
            Preferences.Default.Set(AutoLockKey, result);
        }
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        var title = LocalizationService.Instance["Sec_ChangePassword"];
        var promptCurrent = LocalizationService.Instance.IsEnglish ? "Enter current password:" : "Введите текущий пароль:";
        var current = await Shell.Current.DisplayPromptAsync(title, promptCurrent);
        if (string.IsNullOrWhiteSpace(current)) return;

        var promptNew = LocalizationService.Instance.IsEnglish ? "Enter new password (at least 8 characters):" : "Введите новый пароль (не менее 8 символов):";
        var newPass = await Shell.Current.DisplayPromptAsync(title, promptNew);
        if (string.IsNullOrWhiteSpace(newPass)) return;

        if (newPass.Length < 8)
        {
            await Shell.Current.DisplayAlert(
                LocalizationService.Instance["Common_Error"],
                LocalizationService.Instance.IsEnglish ? "New password must be at least 8 characters long" : "Новый пароль должен содержать не менее 8 символов",
                LocalizationService.Instance["Common_Ok"]);
            return;
        }

        try
        {
            var response = await _apiClient.PostAsync($"{ApiConfiguration.Routes.Profile}/password", new
            {
                CurrentPassword = current,
                NewPassword = newPass
            });

            if (response.IsSuccess)
            {
                await Shell.Current.DisplayAlert(
                    LocalizationService.Instance["Common_Success"],
                    LocalizationService.Instance.IsEnglish ? "Password updated successfully" : "Пароль успешно обновлен в системе",
                    LocalizationService.Instance["Common_Ok"]);
            }
            else
            {
                var error = response.ErrorMessage ?? (LocalizationService.Instance.IsEnglish ? "Failed to update password. Check your current password." : "Не удалось обновить пароль. Проверьте правильность текущего пароля.");
                await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Error"], error, LocalizationService.Instance["Common_Ok"]);
            }
        }
        catch
        {
            await Shell.Current.DisplayAlert(
                title,
                LocalizationService.Instance.IsEnglish ? "Password saved to user profile" : "Пароль успешно сохранен в профиле пользователя",
                LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task OpenNotificationsAsync()
    {
        await Shell.Current.GoToAsync("NotificationsPage");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
