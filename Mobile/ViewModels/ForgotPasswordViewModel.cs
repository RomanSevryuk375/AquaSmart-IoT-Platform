using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _emailOrPhone = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _digit1 = string.Empty;
    [ObservableProperty]
    private string _digit2 = string.Empty;
    [ObservableProperty]
    private string _digit3 = string.Empty;
    [ObservableProperty]
    private string _digit4 = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmNewPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep1))]
    [NotifyPropertyChangedFor(nameof(IsStep2))]
    [NotifyPropertyChangedFor(nameof(IsStep3))]
    [NotifyPropertyChangedFor(nameof(IsStep4))]
    private int _step = 1; // 1: request code, 2: enter pin, 3: set new password, 4: success

    public bool IsStep1 => Step == 1;
    public bool IsStep2 => Step == 2;
    public bool IsStep3 => Step == 3;
    public bool IsStep4 => Step == 4;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public ForgotPasswordViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task SendCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(EmailOrPhone))
        {
            ErrorMessage = "Введите email или телефон";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        await _authService.RequestPasswordResetAsync(EmailOrPhone);
        IsBusy = false;

        Step = 2;
    }

    [RelayCommand]
    private void VerifyCode()
    {
        ErrorMessage = null;
        Step = 3;
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "Введите новый пароль";
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return;
        }

        IsBusy = true;
        var (success, error) = await _authService.ResetPasswordAsync("1234", NewPassword);
        IsBusy = false;

        if (success)
        {
            Step = 4;
        }
        else
        {
            ErrorMessage = error ?? "Не удалось обновить пароль";
        }
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (Step > 1 && Step < 4)
        {
            Step--;
        }
        else
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
