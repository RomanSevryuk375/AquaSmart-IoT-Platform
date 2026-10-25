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
    [NotifyPropertyChangedFor(nameof(PasswordEyeIcon))]
    private bool _isPasswordHidden = true;

    public string PasswordEyeIcon => IsPasswordHidden ? "icon_eye.png" : "icon_eye_off.png";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConfirmPasswordEyeIcon))]
    private bool _isConfirmPasswordHidden = true;

    public string ConfirmPasswordEyeIcon => IsConfirmPasswordHidden ? "icon_eye.png" : "icon_eye_off.png";

    [ObservableProperty]
    private bool _isBusy;

    // Field-level errors (displayed directly under each input element)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFullNameError))]
    private string? _fullNameError;
    public bool HasFullNameError => !string.IsNullOrEmpty(FullNameError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmailError))]
    private string? _emailError;
    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoneError))]
    private string? _phoneError;
    public bool HasPhoneError => !string.IsNullOrEmpty(PhoneError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError))]
    private string? _passwordError;
    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConfirmPasswordError))]
    private string? _confirmPasswordError;
    public bool HasConfirmPasswordError => !string.IsNullOrEmpty(ConfirmPasswordError);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTermsError))]
    private string? _termsError;
    public bool HasTermsError => !string.IsNullOrEmpty(TermsError);

    // General error (network issues, unexpected server error)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public RegistrationViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    partial void OnFullNameChanged(string value) => FullNameError = null;
    partial void OnEmailChanged(string value) => EmailError = null;
    partial void OnPhoneChanged(string value) => PhoneError = null;
    partial void OnPasswordChanged(string value) => PasswordError = null;
    partial void OnConfirmPasswordChanged(string value) => ConfirmPasswordError = null;
    partial void OnAgreeToTermsChanged(bool value) => TermsError = null;

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

    private void ClearAllErrors()
    {
        FullNameError = null;
        EmailError = null;
        PhoneError = null;
        PasswordError = null;
        ConfirmPasswordError = null;
        TermsError = null;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        ClearAllErrors();

        bool hasErrors = false;

        // 1. Validate Full Name
        if (!Helpers.ValidationHelper.ValidateFullName(FullName, out var nameError))
        {
            FullNameError = nameError;
            hasErrors = true;
        }

        // 2. Validate Email
        if (!Helpers.ValidationHelper.ValidateEmail(Email, out var emailError))
        {
            EmailError = emailError;
            hasErrors = true;
        }

        // 3. Validate Phone
        if (!Helpers.ValidationHelper.ValidatePhone(Phone, out var phoneError))
        {
            PhoneError = phoneError;
            hasErrors = true;
        }

        // 4. Validate Password
        if (!Helpers.ValidationHelper.ValidatePassword(Password, out var passError))
        {
            PasswordError = passError;
            hasErrors = true;
        }

        // 5. Validate Confirm Password match
        if (!Helpers.ValidationHelper.ValidateConfirmPassword(Password, ConfirmPassword, out var confirmError))
        {
            ConfirmPasswordError = confirmError;
            hasErrors = true;
        }

        // 6. Check Terms Agreement
        if (!AgreeToTerms)
        {
            TermsError = "Необходимо согласиться с условиями использования сервиса";
            hasErrors = true;
        }

        if (hasErrors)
        {
            // Do not show modal popups - all errors are explained under the respective fields.
            return;
        }

        try
        {
            IsBusy = true;

            var (success, error) = await _authService.RegisterAsync(FullName, Email, Phone, Password);
            if (success)
            {
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            else
            {
                // Deconstruct server validation errors and place them under each field in Russian
                Helpers.ValidationHelper.MapServerRegistrationErrors(
                    error,
                    out var fnError,
                    out var emError,
                    out var phError,
                    out var pwError,
                    out var genError);

                FullNameError = fnError;
                EmailError = emError;
                PhoneError = phError;
                PasswordError = pwError;

                // If error is not a field validation error, display a clear server issue message
                if (!string.IsNullOrEmpty(genError))
                {
                    ErrorMessage = genError;
                }
                else if (string.IsNullOrEmpty(fnError) &&
                         string.IsNullOrEmpty(emError) &&
                         string.IsNullOrEmpty(phError) &&
                         string.IsNullOrEmpty(pwError))
                {
                    ErrorMessage = "Проблемы с сервером. Пожалуйста, попробуйте позже или проверьте подключение к сети.";
                }
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
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
