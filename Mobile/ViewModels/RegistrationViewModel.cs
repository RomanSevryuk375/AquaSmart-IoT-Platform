using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

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
