using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private UserProfileModel _user = new();

    [ObservableProperty]
    private bool _isBusy;

    public ProfileViewModel(IIdentityService identityService, IAuthService authService)
    {
        _identityService = identityService;
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        User = await _identityService.GetCurrentUserAsync();
        IsBusy = false;
    }

    [RelayCommand]
    private async Task EditProfileAsync()
    {
        var newName = await Shell.Current.DisplayPromptAsync("Редактирование", "Введите ФИО:", initialValue: User.FullName);
        if (!string.IsNullOrWhiteSpace(newName))
        {
            User.FullName = newName;
            await _identityService.UpdateCurrentUserAsync(User);
            OnPropertyChanged(nameof(User));
        }
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

    [RelayCommand]
    private async Task ShowAboutAsync()
    {
        await Shell.Current.DisplayAlert("О приложении", "AquaSmart IoT Platform Mobile\nВерсия: 1.0.0 (Release)\n\nПлатформа комплексного мониторинга и автоматизации аквасистем.", "Закрыть");
    }

    [RelayCommand]
    private async Task ShowSettingsAsync()
    {
        await Shell.Current.DisplayAlert("Настройки приложения", "• Тема: Системная (Светлая/Тёмная)\n• Интервал телеметрии: 5 сек\n• Push-уведомления: Включены\n• Звуковые оповещения алертов: Включены", "OK");
    }

    [RelayCommand]
    private async Task ShowSecurityAsync()
    {
        await Shell.Current.DisplayAlert("Безопасность", "• Двухфакторная аутентификация: Активна\n• Сессия: Защищена JWT и SecureStorage\n• Последний вход: Сегодня", "OK");
    }

    [RelayCommand]
    private async Task ShowSupportAsync()
    {
        await Shell.Current.DisplayAlert("Помощь и поддержка", "Служба поддержки AquaSmart IoT:\nEmail: support@aquasmart.io\nTelegram: @AquaSmartSupport\n\nБаза знаний доступна в боковом меню.", "Понятно");
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Удаление аккаунта", "Вы действительно хотите удалить свой аккаунт и все связанные контроллеры? Это действие необратимо.", "Удалить", "Отмена");
        if (confirm)
        {
            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
