using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

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
