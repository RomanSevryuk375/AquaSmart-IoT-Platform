using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;
using Mobile.Views;

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
    [NotifyPropertyChangedFor(nameof(HasAvatar))]
    private string? _avatarPath;

    public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarPath) && File.Exists(AvatarPath);

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
        AvatarPath = user.AvatarPath;
        OnPropertyChanged(nameof(HasAvatar));
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
            Phone = !string.IsNullOrWhiteSpace(cleanDigitsPhone) ? cleanDigitsPhone : normalizedPhone,
            AvatarPath = AvatarPath
        };

        await _identityService.UpdateCurrentUserAsync(profile);
        IsBusy = false;

        await Shell.Current.DisplayAlert("Успешно", "Профиль успешно обновлен", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ChangePhotoAsync()
    {
        await Shell.Current.GoToAsync(nameof(ChangeAvatarPage));
    }

    private async Task SaveSelectedAvatarAsync(FileResult photo)
    {
        IsBusy = true;
        try
        {
            var fileName = $"user_avatar_{DateTime.UtcNow.Ticks}.jpg";
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using (var sourceStream = await photo.OpenReadAsync())
            using (var targetStream = File.Create(targetPath))
            {
                await sourceStream.CopyToAsync(targetStream);
            }

            if (!string.IsNullOrWhiteSpace(AvatarPath) && File.Exists(AvatarPath) && AvatarPath != targetPath)
            {
                try { File.Delete(AvatarPath); } catch { }
            }

            AvatarPath = targetPath;
            OnPropertyChanged(nameof(HasAvatar));

            var user = await _identityService.GetCurrentUserAsync();
            user.AvatarPath = targetPath;
            await _identityService.UpdateCurrentUserAsync(user);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить фото: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
