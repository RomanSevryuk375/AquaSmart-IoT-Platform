using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class ChangeAvatarViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAvatar))]
    private string? _avatarPath;

    public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarPath) && File.Exists(AvatarPath);

    [ObservableProperty]
    private bool _isBusy;

    public ChangeAvatarViewModel(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        var user = await _identityService.GetCurrentUserAsync();
        AvatarPath = user.AvatarPath;
        OnPropertyChanged(nameof(HasAvatar));
        IsBusy = false;
    }

    [RelayCommand]
    private async Task PickFromGalleryAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = LocalizationService.Instance.IsEnglish ? "Select profile photo" : "Выберите фото профиля"
            });

            if (photo != null)
            {
                IsBusy = true;
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

                await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Success"], LocalizationService.Instance["Avatar_Updated"], LocalizationService.Instance["Common_Ok"]);
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

    [RelayCommand]
    private async Task DeletePhotoAsync()
    {
        if (!HasAvatar)
        {
            await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Attention"], LocalizationService.Instance["Avatar_NoPhoto"], LocalizationService.Instance["Common_Ok"]);
            return;
        }

        var confirm = await Shell.Current.DisplayAlert(LocalizationService.Instance["Avatar_DeleteConfirmTitle"], LocalizationService.Instance["Avatar_DeleteConfirmDesc"], LocalizationService.Instance["Common_Delete"], LocalizationService.Instance["Common_Cancel"]);
        if (confirm)
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

            await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Success"], LocalizationService.Instance["Avatar_Deleted"], LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
