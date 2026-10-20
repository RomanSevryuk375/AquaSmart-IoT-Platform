using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private string _userName = "Алексей Смирнов";

    [ObservableProperty]
    private string _userEmail = "alex.smirnov@aquasmart.io";

    [ObservableProperty]
    private string _userRole = "Аквариумист";

    public AppShellViewModel(IIdentityService identityService)
    {
        _identityService = identityService;
        LoadUserData();
    }

    private async void LoadUserData()
    {
        var profile = await _identityService.GetCurrentUserAsync();
        UserName = profile.FullName;
        UserEmail = profile.Email;
        UserRole = profile.Role;
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
