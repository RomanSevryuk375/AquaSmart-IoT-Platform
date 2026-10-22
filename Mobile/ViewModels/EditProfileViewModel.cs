using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class EditProfileViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _initials = "Р";

    [ObservableProperty]
    private bool _isBusy;

    public EditProfileViewModel(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        var user = await _identityService.GetCurrentUserAsync();
        FullName = string.IsNullOrWhiteSpace(user.FullName) ? "Роман" : user.FullName;
        Email = string.IsNullOrWhiteSpace(user.Email) ? "roman.romanov@gmail.com" : user.Email;
        Phone = string.IsNullOrWhiteSpace(user.Phone) ? "+375 (29) 123-45-67" : user.Phone;
        Initials = user.Initials;
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, укажите ваше имя", "OK");
            return;
        }

        var normalizedPhone = Phone?.Trim() ?? string.Empty;
        var cleanDigitsPhone = normalizedPhone.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");

        IsBusy = true;
        var profile = new UserProfileModel
        {
            FullName = FullName.Trim(),
            Email = Email.Trim(),
            Phone = !string.IsNullOrWhiteSpace(cleanDigitsPhone) ? cleanDigitsPhone : normalizedPhone
        };

        await _identityService.UpdateCurrentUserAsync(profile);
        IsBusy = false;

        await Shell.Current.DisplayAlert("Успешно", "Профиль успешно обновлен", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ChangePhotoAsync()
    {
        var action = await Shell.Current.DisplayActionSheet("Фото профиля", "Отмена", "Удалить текущее фото", "Выбрать из галереи", "Сделать снимок");
        if (action == "Выбрать из галереи" || action == "Сделать снимок")
        {
            await Shell.Current.DisplayAlert("Фото профиля", "Новая фотография успешно установлена", "OK");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
