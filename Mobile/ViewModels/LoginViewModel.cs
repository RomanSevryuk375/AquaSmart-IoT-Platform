using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

        if (string.IsNullOrWhiteSpace(EmailOrLogin))
        {
            ErrorMessage = "Укажите email или логин";
            await Shell.Current.DisplayAlert("Внимание", ErrorMessage, "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите пароль";
            await Shell.Current.DisplayAlert("Внимание", ErrorMessage, "OK");
            return;
        }

        if (Password.Length < Helpers.ValidationHelper.MinPasswordLength)
        {
            ErrorMessage = $"Пароль должен содержать не менее {Helpers.ValidationHelper.MinPasswordLength} символов";
            await Shell.Current.DisplayAlert("Внимание", ErrorMessage, "OK");
            return;
        }

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
                ErrorMessage = Helpers.ValidationHelper.LocalizeServerError(error);
                await Shell.Current.DisplayAlert("Ошибка входа", ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = Helpers.ValidationHelper.LocalizeServerError(ex.Message);
            await Shell.Current.DisplayAlert("Ошибка", ErrorMessage, "OK");
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
