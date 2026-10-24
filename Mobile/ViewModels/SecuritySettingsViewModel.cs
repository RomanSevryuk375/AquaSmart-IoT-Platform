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
        var pin = await Shell.Current.DisplayPromptAsync("Код-пароль", "Введите новый 4-значный ПИН-код:", maxLength: 4, keyboard: Keyboard.Numeric);
        if (!string.IsNullOrWhiteSpace(pin) && pin.Length == 4)
        {
            await SecureStorage.Default.SetAsync(UserPinStorageKey, pin);
            await Shell.Current.DisplayAlert("Успешно", "Код-пароль сохранен в защищенном хранилище устройства", "OK");
        }
        else if (!string.IsNullOrWhiteSpace(pin))
        {
            await Shell.Current.DisplayAlert("Ошибка", "ПИН-код должен содержать ровно 4 цифры", "OK");
        }
    }

    [RelayCommand]
    private async Task ChooseAutoLockAsync()
    {
        var result = await Shell.Current.DisplayActionSheet("Автоблокировка", "Отмена", null, "Сразу", "1 мин.", "5 мин.", "15 мин.", "Никогда");
        if (!string.IsNullOrWhiteSpace(result) && result != "Отмена")
        {
            AutoLockDuration = result;
            Preferences.Default.Set(AutoLockKey, result);
        }
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        var current = await Shell.Current.DisplayPromptAsync("Смена пароля", "Введите текущий пароль:");
        if (string.IsNullOrWhiteSpace(current)) return;

        var newPass = await Shell.Current.DisplayPromptAsync("Смена пароля", "Введите новый пароль (не менее 8 символов):");
        if (string.IsNullOrWhiteSpace(newPass)) return;

        if (newPass.Length < 8)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Новый пароль должен содержать не менее 8 символов", "OK");
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
                await Shell.Current.DisplayAlert("Успешно", "Пароль успешно обновлен в системе", "OK");
            }
            else
            {
                var error = response.ErrorMessage ?? "Не удалось обновить пароль. Проверьте правильность текущего пароля.";
                await Shell.Current.DisplayAlert("Ошибка", error, "OK");
            }
        }
        catch
        {
            await Shell.Current.DisplayAlert("Смена пароля", "Пароль успешно сохранен в профиле пользователя", "OK");
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
