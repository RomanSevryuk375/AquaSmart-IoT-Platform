namespace Mobile.Models;

public class UserProfileModel
{
    public string FullName { get; set; } = "Алексей Смирнов";
    public string Email { get; set; } = "alex.smirnov@aquasmart.io";
    public string Phone { get; set; } = "+7 (999) 123-45-67";
    public string Role { get; set; } = "Владелец аквасистемы";
    public bool PushNotificationsEnabled { get; set; } = true;
    public bool CriticalAlertsOnly { get; set; } = false;
    public bool DarkThemeEnabled { get; set; } = false;
}
