namespace Mobile.Models;

public class UserProfileModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "Пользователь";
    public string? AvatarPath { get; set; }
    public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarPath) && File.Exists(AvatarPath);
    public bool PushNotificationsEnabled { get; set; } = true;
    public bool CriticalAlertsOnly { get; set; } = false;
    public bool DarkThemeEnabled { get; set; } = false;

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
