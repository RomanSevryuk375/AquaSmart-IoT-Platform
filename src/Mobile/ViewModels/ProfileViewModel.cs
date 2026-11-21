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
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private bool _hasAvatar;

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
        var currentUser = await _identityService.GetCurrentUserAsync();
        User = new UserProfileModel
        {
            FullName = currentUser.FullName,
            Email = currentUser.Email,
            Phone = currentUser.Phone,
            Role = currentUser.Role,
            AvatarPath = currentUser.AvatarPath,
            PushNotificationsEnabled = currentUser.PushNotificationsEnabled,
            CriticalAlertsOnly = currentUser.CriticalAlertsOnly,
            DarkThemeEnabled = currentUser.DarkThemeEnabled
        };

        HasAvatar = !string.IsNullOrWhiteSpace(User.AvatarPath) && File.Exists(User.AvatarPath);
        AvatarSource = HasAvatar ? ImageSource.FromFile(User.AvatarPath!) : null;
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

    [RelayCommand]
    private async Task LinkTelegramAsync()
    {
        try
        {
            IsBusy = true;
            var link = await _identityService.GenerateTelegramLinkTokenAsync();
            if (!string.IsNullOrWhiteSpace(link))
            {
                await Launcher.OpenAsync(new Uri(link));
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Telegram",
                    LocalizationService.Instance.IsEnglish
                        ? "Failed to generate bot link. Check server connection."
                        : "Не удалось создать ссылку для привязки бота. Проверьте соединение с сервером.",
                    LocalizationService.Instance["Common_Ok"]);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Error"], ex.Message, LocalizationService.Instance["Common_Ok"]);
        }
        finally
        {
            IsBusy = false;
        }
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
        var answer = await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Profile_LogoutConfirmTitle"],
            LocalizationService.Instance["Profile_LogoutConfirmMsg"],
            LocalizationService.Instance["Common_Yes"],
            LocalizationService.Instance["Common_Cancel"]);
        if (answer)
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
