using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;
using Mobile.Services.Dto.Notification;
using Mobile.Services.Interfaces;
using Mobile.Views;

namespace Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private UserProfileModel _user = new();

    [ObservableProperty]
    private bool _isBusy;

    public ProfileViewModel(IIdentityService identityService, IAuthService authService, IApiClient apiClient)
    {
        _identityService = identityService;
        _authService = authService;
        _apiClient = apiClient;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        User = await _identityService.GetCurrentUserAsync();
        // If user full name is empty in mock/first run, provide pleasant default from Figma
        if (string.IsNullOrWhiteSpace(User.FullName))
        {
            User.FullName = "Роман";
            User.Email = "roman.romanov@gmail.com";
            User.Phone = "+375 (29) 123-45-67";
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task GoToEditProfileAsync()
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }

    [RelayCommand]
    private async Task GoToChangeAvatarAsync()
    {
        await Shell.Current.GoToAsync(nameof(ChangeAvatarPage));
    }

    [RelayCommand]
    private async Task OpenNotificationsAsync()
    {
        await Shell.Current.GoToAsync(nameof(NotificationsPage));
    }

    private static string GetLevelIcon(int level) => level switch
    {
        3 => "🔴",
        2 => "🟡",
        4 => "❌",
        _ => "ℹ️"
    };

    [RelayCommand]
    private async Task GoToControllersAsync()
    {
        await Shell.Current.GoToAsync(nameof(ControllersPage));
    }

    [RelayCommand]
    private async Task GoToAppSettingsAsync()
    {
        await Shell.Current.GoToAsync(nameof(AppSettingsPage));
    }

    [RelayCommand]
    private async Task GoToDataHistoryAsync()
    {
        await Shell.Current.GoToAsync(nameof(DataHistoryPage));
    }

    [RelayCommand]
    private async Task GoToSecurityAsync()
    {
        await Shell.Current.GoToAsync(nameof(SecuritySettingsPage));
    }

    [RelayCommand]
    private async Task GoToHelpSupportAsync()
    {
        await Shell.Current.GoToAsync(nameof(HelpSupportPage));
    }

    [RelayCommand]
    private async Task GoToAboutAppAsync()
    {
        await Shell.Current.GoToAsync(nameof(AboutAppPage));
    }

    [RelayCommand]
    private async Task GoToDeleteAccountAsync()
    {
        await Shell.Current.GoToAsync(nameof(DeleteAccountPage));
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var answer = await Shell.Current.DisplayAlert("Выход", "Вы действительно хотите выйти из аккаунта?", "Да", "Отмена");
        if (answer)
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
