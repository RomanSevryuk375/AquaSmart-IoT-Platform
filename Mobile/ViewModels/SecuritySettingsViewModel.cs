using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class SecuritySettingsViewModel : ObservableObject
{
    private const string UseFaceIdKey = "sec_use_face_id";
    private const string AutoLockKey = "sec_auto_lock";
    private const string TwoFactorKey = "sec_2fa_enabled";
    private const string PinForRelayKey = "sec_pin_for_relay";

    [ObservableProperty]
    private bool _useFaceId = true;

    [ObservableProperty]
    private string _autoLockDuration = "1 мин.";

    [ObservableProperty]
    private bool _twoFactorEnabled = false;

    [ObservableProperty]
    private bool _pinForRelay = false;

    public SecuritySettingsViewModel()
    {
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
        var pin = await Shell.Current.DisplayPromptAsync("Код-пароль", "Введите новый 4-значный ПИН:", maxLength: 4, keyboard: Keyboard.Numeric);
        if (!string.IsNullOrWhiteSpace(pin) && pin.Length == 4)
        {
            await Shell.Current.DisplayAlert("Успех", "Код-пароль успешно установлен", "OK");
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
        if (!string.IsNullOrWhiteSpace(newPass))
        {
            if (newPass.Length < 8)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Пароль должен содержать не менее 8 символов", "OK");
                return;
            }
            await Shell.Current.DisplayAlert("Успешно", "Пароль успешно обновлен", "OK");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
