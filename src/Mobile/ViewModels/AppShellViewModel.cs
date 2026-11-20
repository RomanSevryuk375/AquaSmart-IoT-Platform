using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _userName = LocalizationService.Instance.IsEnglish ? "User" : "Пользователь";

    [ObservableProperty]
    private string _userEmail = string.Empty;

    [ObservableProperty]
    private string _userRole = LocalizationService.Instance.IsEnglish ? "User" : "Пользователь";

    [ObservableProperty]
    private string _userInitials = "👤";

    [ObservableProperty]
    private string? _userAvatarPath;

    [ObservableProperty]
    private bool _hasUserAvatar;

    public AppShellViewModel(IIdentityService identityService, IAuthService authService)
    {
        _identityService = identityService;
        _authService = authService;
        RefreshUserData();
    }

    public void RefreshUserData()
    {
        _ = LoadUserDataAsync();
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            var profile = await _identityService.GetCurrentUserAsync();
            var defaultUser = LocalizationService.Instance.IsEnglish ? "User" : "Пользователь";
            UserName = !string.IsNullOrWhiteSpace(profile.FullName) ? profile.FullName : defaultUser;
            UserEmail = profile.Email;
            UserRole = !string.IsNullOrWhiteSpace(profile.Role) ? profile.Role : defaultUser;
            UserInitials = profile.Initials;
            UserAvatarPath = profile.AvatarPath;
            HasUserAvatar = profile.HasAvatar;
        }
        catch
        {
        }
    }

    [RelayCommand]
    private async Task GoToProfileAsync()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync("ProfilePage");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var answer = await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Profile_LogoutConfirmTitle"],
            LocalizationService.Instance["Profile_LogoutConfirmMsg"],
            LocalizationService.Instance["Common_Yes"],
            LocalizationService.Instance["Common_Cancel"]);
        if (answer)
        {
            Shell.Current.FlyoutIsPresented = false;
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
