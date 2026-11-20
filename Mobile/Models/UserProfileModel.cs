using CommunityToolkit.Mvvm.ComponentModel;

namespace Mobile.Models;

public partial class UserProfileModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _role = Services.LocalizationService.Instance.IsEnglish ? "User" : "Пользователь";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAvatar))]
    private string? _avatarPath;

    public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarPath) && File.Exists(AvatarPath);

    [ObservableProperty]
    private bool _pushNotificationsEnabled = true;

    [ObservableProperty]
    private bool _criticalAlertsOnly = false;

    [ObservableProperty]
    private bool _darkThemeEnabled = false;

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                if (!string.IsNullOrWhiteSpace(Email) && Email.Length >= 2)
                    return Email[..2].ToUpperInvariant();
                return "👤";
            }
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
            if (parts.Length == 1 && parts[0].Length >= 2)
                return parts[0][..2].ToUpperInvariant();
            return parts[0].ToUpperInvariant();
        }
    }
}
