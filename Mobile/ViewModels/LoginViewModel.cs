using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ISecurityService _securityService;

    [ObservableProperty]
    private string _emailOrLogin = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordEyeIcon))]
    private bool _isPasswordHidden = true;

    public string PasswordEyeIcon => IsPasswordHidden ? "icon_eye.png" : "icon_eye_off.png";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _canUseBiometrics;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmailOrLoginError))]
    private string? _emailOrLoginError;
    public bool HasEmailOrLoginError => !string.IsNullOrEmpty(EmailOrLoginError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError))]
    private string? _passwordError;
    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public LoginViewModel(IAuthService authService, ISecurityService securityService)
    {
        _authService = authService;
        _securityService = securityService;
        _ = CheckBiometricsAsync();
    }

    private async Task CheckBiometricsAsync()
    {
        CanUseBiometrics = await _securityService.CanUseBiometricsOrPinAsync();
    }

    [RelayCommand]
    private async Task BiometricLoginAsync()
    {
        if (IsBusy) return;
        var success = await _securityService.AuthenticateBiometricOrPinAsync();
        if (success)
        {
            await Shell.Current.GoToAsync("//DashboardPage");
        }
    }

    partial void OnEmailOrLoginChanged(string value)
    {
        EmailOrLoginError = null;
        ErrorMessage = null;
    }

    partial void OnPasswordChanged(string value)
    {
        PasswordError = null;
        ErrorMessage = null;
    }

    [RelayCommand]
    private void TogglePassword()
    {
        IsPasswordHidden = !IsPasswordHidden;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        EmailOrLoginError = null;
        PasswordError = null;
        ErrorMessage = null;

        bool hasErrors = false;

        if (string.IsNullOrWhiteSpace(EmailOrLogin))
        {
            EmailOrLoginError = "Укажите email или телефон";
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Введите пароль";
            hasErrors = true;
        }
        else if (Password.Length < Helpers.ValidationHelper.MinPasswordLength)
        {
            PasswordError = $"Пароль должен содержать не менее {Helpers.ValidationHelper.MinPasswordLength} символов";
            hasErrors = true;
        }

        if (hasErrors)
        {
            // Do not show modal popups - errors are displayed under each field
            return;
        }

        try
        {
            IsBusy = true;

            var (success, error) = await _authService.LoginAsync(EmailOrLogin, Password);
            if (success)
            {
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                var localized = Helpers.ValidationHelper.LocalizeServerError(error);
                ErrorMessage = !string.IsNullOrWhiteSpace(localized)
                    ? localized
                    : "Проблемы с сервером. Пожалуйста, попробуйте позже или проверьте подключение к сети.";
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Проблемы с сервером или подключением к сети. Пожалуйста, попробуйте позже.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToRegistrationAsync()
    {
        await Shell.Current.GoToAsync("RegistrationPage");
    }

    [RelayCommand]
    private async Task GoToForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync("ForgotPasswordPage");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
