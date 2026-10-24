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
        try
        {
            string[] buttons = HasAvatar
                ? new[] { "Выбрать из галереи", "Сделать снимок", "Удалить текущее фото" }
                : new[] { "Выбрать из галереи", "Сделать снимок" };

            var action = await Shell.Current.DisplayActionSheet("Фото профиля", "Отмена", null, buttons);
            if (string.IsNullOrWhiteSpace(action) || action == "Отмена")
            {
                return;
            }

            if (action == "Выбрать из галереи")
            {
                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите фото профиля"
                });

                if (photo != null)
                {
                    await SaveSelectedAvatarAsync(photo);
                }
            }
            else if (action == "Сделать снимок")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.Camera>();
                    }

                    if (status == PermissionStatus.Granted)
                    {
                        var photo = await MediaPicker.Default.CapturePhotoAsync();
                        if (photo != null)
                        {
                            await SaveSelectedAvatarAsync(photo);
                        }
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Доступ ограничен", "Для съемки фото предоставьте разрешение на камеру", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Камера", "Съемка фото не поддерживается на данном устройстве", "OK");
                }
            }
            else if (action == "Удалить текущее фото")
            {
                if (!string.IsNullOrWhiteSpace(AvatarPath) && File.Exists(AvatarPath))
                {
                    try { File.Delete(AvatarPath); } catch { }
                }
                AvatarPath = null;
                OnPropertyChanged(nameof(HasAvatar));

                var user = await _identityService.GetCurrentUserAsync();
                user.AvatarPath = null;
                await _identityService.UpdateCurrentUserAsync(user);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
        }
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
