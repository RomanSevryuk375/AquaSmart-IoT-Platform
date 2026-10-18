using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _emailOrLogin = "alex.smirnov@aquasmart.io";

    [ObservableProperty]
    private string _password = "password123";

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
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

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var (success, error) = await _authService.LoginAsync(EmailOrLogin, Password);
            if (success)
            {
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                ErrorMessage = error;
                await Shell.Current.DisplayAlert("Ошибка входа", error ?? "Неверные учетные данные", "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
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

public partial class RegistrationViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _agreeToTerms = true;

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public RegistrationViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private void TogglePassword()
    {
        IsPasswordHidden = !IsPasswordHidden;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Внимание", "Заполните все обязательные поля", "OK");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await Shell.Current.DisplayAlert("Внимание", "Пароли не совпадают", "OK");
            return;
        }

        if (!AgreeToTerms)
        {
            await Shell.Current.DisplayAlert("Внимание", "Необходимо согласиться с условиями использования сервиса", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var (success, error) = await _authService.RegisterAsync(FullName, Email, Phone, Password);
            if (success)
            {
                await Shell.Current.DisplayAlert("Успешно", "Регистрация завершена! Добро пожаловать в Smart Aqua.", "OK");
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                ErrorMessage = error;
                await Shell.Current.DisplayAlert("Ошибка регистрации", error ?? "Не удалось зарегистрироваться", "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

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
