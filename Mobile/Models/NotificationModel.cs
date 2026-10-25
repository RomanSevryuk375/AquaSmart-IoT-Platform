using CommunityToolkit.Mvvm.ComponentModel;

namespace Mobile.Models;

public partial class NotificationModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Level { get; set; } // 0: Info, 1: Warning, 2: Error, 3: Success

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CardBackgroundColor))]
    [NotifyPropertyChangedFor(nameof(CardBorderColor))]
    [NotifyPropertyChangedFor(nameof(HasUnreadDot))]
    private bool _isRead;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt;
            if (diff.TotalMinutes < 2) return "Только что";
            if (diff.TotalHours < 1) return $"{(int)diff.TotalMinutes} мин назад";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} ч назад";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} дн назад";
            return CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        }
    }

    public string LevelBadgeText => Level switch
    {
        2 => "Ошибка",
        1 => "Внимание",
        3 => "Успешно",
        _ => "Инфо"
    };

    public string LevelBadgeBgColor => Level switch
    {
        2 => "#FDE8E8",
        1 => "#FFF3CD",
        3 => "#DEF7EC",
        _ => "#EBF0FA"
    };

    public string LevelBadgeTextColor => Level switch
    {
        2 => "#C81E1E",
        1 => "#9F580A",
        3 => "#03543F",
        _ => "#4152B0"
    };

    public string CardBackgroundColor => IsRead ? "#FFFFFF" : "#F7F8FE";
    public string CardBorderColor => IsRead ? "#E4E7F4" : "#4152B0";
    public bool HasUnreadDot => !IsRead;
}
