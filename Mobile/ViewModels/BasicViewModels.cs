using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;

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

public partial class MainViewModel : ObservableObject
{
    [RelayCommand]
    private async Task GoToOnboardingAsync()
    {
        await Shell.Current.GoToAsync("OnboardingPage");
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("LoginPage");
    }
}

public partial class OnboardingViewModel : ObservableObject
{
    [ObservableProperty]
    private int _currentPosition = 0;

    [ObservableProperty]
    private List<OnboardingSlideModel> _slides = new();

    public OnboardingViewModel()
    {
        Slides = new List<OnboardingSlideModel>
        {
            new OnboardingSlideModel
            {
                Title = "Умный мониторинг аквасистем",
                Description = "Контролируйте температуру, pH, кислород O2 и минерализацию воды в реальном времени с точных датчиков.",
                IconEmoji = "🐠",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = "Автоматизация и реле",
                Description = "Управляйте аэраторами, обогревателями, фильтрами и кормушками прямо со смартфона или по гибким сценариям.",
                IconEmoji = "⚡",
                AccentColor = "#5364C7"
            },
            new OnboardingSlideModel
            {
                Title = "Мгновенные уведомления",
                Description = "Получайте предупреждения о выходе параметров за критические пороги до того, как пострадают гидробионты.",
                IconEmoji = "🔔",
                AccentColor = "#334193"
            },
            new OnboardingSlideModel
            {
                Title = "База знаний и советы",
                Description = "Экспертные рекомендации по видам рыб, растениям и идеальным гидрохимическим условиям для вашего биотопа.",
                IconEmoji = "📖",
                AccentColor = "#2E3A7E"
            }
        };
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    [RelayCommand]
    private async Task NextOrFinishAsync()
    {
        if (CurrentPosition < Slides.Count - 1)
        {
            CurrentPosition++;
        }
        else
        {
            await Shell.Current.GoToAsync("LoginPage");
        }
    }
}
