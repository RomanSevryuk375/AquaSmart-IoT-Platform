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
    private bool _isConfirmPasswordHidden = true;

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
    private void ToggleConfirmPassword()
    {
        IsConfirmPasswordHidden = !IsConfirmPasswordHidden;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        // 1. Validate Full Name (backend: CommonConstants.NameLength = 128)
        if (!Helpers.ValidationHelper.ValidateFullName(FullName, out var nameError))
        {
            ErrorMessage = nameError;
            await Shell.Current.DisplayAlert("Внимание", nameError, "OK");
            return;
        }

        // 2. Validate Email (backend: IdentityConstants.EmailNumberRegex and EmailLength = 256)
        if (!Helpers.ValidationHelper.ValidateEmail(Email, out var emailError))
        {
            ErrorMessage = emailError;
            await Shell.Current.DisplayAlert("Внимание", emailError, "OK");
            return;
        }

        // 3. Validate Phone (backend: IdentityConstants.PhoneNumberRegex +375XXXXXXXXX or 80XXXXXXXXX)
        if (!Helpers.ValidationHelper.ValidatePhone(Phone, out var phoneError))
        {
            ErrorMessage = phoneError;
            await Shell.Current.DisplayAlert("Внимание", phoneError, "OK");
            return;
        }

        // 4. Validate Password (backend: IdentityConstants.PasswordLength >= 8)
        if (!Helpers.ValidationHelper.ValidatePassword(Password, out var passError))
        {
            ErrorMessage = passError;
            await Shell.Current.DisplayAlert("Внимание", passError, "OK");
            return;
        }

        // 5. Validate Confirm Password match
        if (!Helpers.ValidationHelper.ValidateConfirmPassword(Password, ConfirmPassword, out var confirmError))
        {
            ErrorMessage = confirmError;
            await Shell.Current.DisplayAlert("Внимание", confirmError, "OK");
            return;
        }

        // 6. Check Terms Agreement
        if (!AgreeToTerms)
        {
            ErrorMessage = "Необходимо согласиться с условиями использования сервиса";
            await Shell.Current.DisplayAlert("Внимание", ErrorMessage, "OK");
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
                ErrorMessage = Helpers.ValidationHelper.LocalizeServerError(error);
                await Shell.Current.DisplayAlert("Ошибка регистрации", ErrorMessage, "OK");
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
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
