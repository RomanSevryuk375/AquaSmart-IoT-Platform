using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;

namespace Mobile.ViewModels;

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
                Title = LocalizationService.Instance["Onboarding_Slide1Title"],
                Description = LocalizationService.Instance["Onboarding_Slide1Desc"],
                BadgeText = "🛡️",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = LocalizationService.Instance["Onboarding_Slide2Title"],
                Description = LocalizationService.Instance["Onboarding_Slide2Desc"],
                BadgeText = "🌡️",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = LocalizationService.Instance["Onboarding_Slide3Title"],
                Description = LocalizationService.Instance["Onboarding_Slide3Desc"],
                BadgeText = "⚙️",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = LocalizationService.Instance["Onboarding_Slide4Title"],
                Description = LocalizationService.Instance["Onboarding_Slide4Desc"],
                BadgeText = "🤖",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = LocalizationService.Instance["Onboarding_Slide5Title"],
                Description = LocalizationService.Instance["Onboarding_Slide5Desc"],
                BadgeText = "▶️",
                AccentColor = "#4152B0"
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
