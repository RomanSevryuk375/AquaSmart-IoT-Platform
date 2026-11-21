using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class DeleteAccountViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private bool _isBusy;

    public DeleteAccountViewModel(IAuthService authService, IIdentityService identityService)
    {
        _authService = authService;
        _identityService = identityService;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        IsBusy = true;
        await _identityService.ClearTokenAsync();
        await _authService.LogoutAsync();
        IsBusy = false;

        await Shell.Current.DisplayAlert(LocalizationService.Instance["DeleteAcc_DeletedTitle"], LocalizationService.Instance["DeleteAcc_DeletedDesc"], LocalizationService.Instance["Common_Ok"]);
        await Shell.Current.GoToAsync("//MainPage");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
