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
    private string? _errorMessage;

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
    private string? _errorMessage;

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
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private int _step = 1; // 1: request code, 2: enter code & new pass

    [ObservableProperty]
    private bool _isBusy;

    public ForgotPasswordViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task SendCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(EmailOrPhone))
        {
            await Shell.Current.DisplayAlert("Внимание", "Введите email или телефон", "OK");
            return;
        }

        IsBusy = true;
        await _authService.RequestPasswordResetAsync(EmailOrPhone);
        IsBusy = false;

        Step = 2;
        await Shell.Current.DisplayAlert("Код отправлен", "Код восстановления направлен на указанные контактные данные", "OK");
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(NewPassword))
        {
            await Shell.Current.DisplayAlert("Внимание", "Заполните код и новый пароль", "OK");
            return;
        }

        IsBusy = true;
        var (success, error) = await _authService.ResetPasswordAsync(Code, NewPassword);
        IsBusy = false;

        if (success)
        {
            await Shell.Current.DisplayAlert("Успех", "Пароль успешно обновлен!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await Shell.Current.DisplayAlert("Ошибка", error ?? "Неверный код", "OK");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
