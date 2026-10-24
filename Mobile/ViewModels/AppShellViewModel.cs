using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _userName = "Пользователь";

    [ObservableProperty]
    private string _userEmail = string.Empty;

    [ObservableProperty]
    private string _userRole = "Пользователь";

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
            UserName = !string.IsNullOrWhiteSpace(profile.FullName) ? profile.FullName : "Пользователь";
            UserEmail = profile.Email;
            UserRole = !string.IsNullOrWhiteSpace(profile.Role) ? profile.Role : "Пользователь";
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
        var answer = await Shell.Current.DisplayAlert("Выход", "Вы действительно хотите выйти из аккаунта?", "Да", "Отмена");
        if (answer)
        {
            Shell.Current.FlyoutIsPresented = false;
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
