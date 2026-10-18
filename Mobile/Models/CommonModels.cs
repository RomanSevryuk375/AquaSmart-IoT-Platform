namespace Mobile.Models;

public class KnowledgeArticleModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ReadTimeMinutes { get; set; } = 3;
    public string Icon { get; set; } = "📖";
}

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

public class OnboardingSlideModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = "🐠";
    public string BadgeText { get; set; } = "🛡️";
    public string AccentColor { get; set; } = "#4152B0";
}
