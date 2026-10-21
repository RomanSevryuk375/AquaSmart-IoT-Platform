using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private string _userName = "Пользователь";

    [ObservableProperty]
    private string _userEmail = string.Empty;

    [ObservableProperty]
    private string _userRole = "Пользователь";

    [ObservableProperty]
    private string _userInitials = "👤";

    public AppShellViewModel(IIdentityService identityService)
    {
        _identityService = identityService;
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
            await _identityService.ClearTokenAsync();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
