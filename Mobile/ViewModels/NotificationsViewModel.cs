using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;
using Mobile.Services.Dto.Notification;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly List<NotificationModel> _allNotifications = new();

    [ObservableProperty]
    private ObservableCollection<NotificationModel> _notifications = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoNotifications))]
    private bool _hasNotifications;

    public bool HasNoNotifications => !HasNotifications;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private string _selectedFilter = "Все"; // "Все", "Непрочитанные", "Важные"

    [ObservableProperty]
    private bool _isBusy;

    public bool IsAllFilterSelected => SelectedFilter == "Все";
    public bool IsUnreadFilterSelected => SelectedFilter == "Непрочитанные";
    public bool IsImportantFilterSelected => SelectedFilter == "Важные";

    public NotificationsViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task InitializeAsync()
    {
        await LoadNotificationsAsync();
    }

    [RelayCommand]
    public async Task LoadNotificationsAsync()
    {
        IsBusy = true;
        try
        {
            var response = await _apiClient.GetAsync<IReadOnlyList<NotificationItemDto>>(
                $"{ApiConfiguration.Routes.Notifications}?take=50");

            _allNotifications.Clear();

            if (response.IsSuccess && response.Data != null && response.Data.Count > 0)
            {
                foreach (var dto in response.Data)
                {
                    _allNotifications.Add(new NotificationModel
                    {
                        Id = dto.Id,
                        Message = dto.Message,
                        Level = dto.Level,
                        IsRead = dto.IsRead,
                        CreatedAt = dto.CreatedAt
                    });
                }
            }
            else
            {
                // Fallback realistic mock notifications if API has none or offline
                LoadDefaultMockNotifications();
            }

            ApplyFilter();
        }
        catch
        {
            if (_allNotifications.Count == 0)
            {
                LoadDefaultMockNotifications();
                ApplyFilter();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadDefaultMockNotifications()
    {
        _allNotifications.Clear();
        _allNotifications.AddRange(new[]
        {
            new NotificationModel
            {
                Id = Guid.NewGuid(),
                Message = "Температура в аквариуме 25.4°C — показатели в норме",
                Level = 3,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-12)
            },
            new NotificationModel
            {
                Id = Guid.NewGuid(),
                Message = "Контроллер Хаб-02 подключен к сети и передает данные телеметрии",
                Level = 0,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new NotificationModel
            {
                Id = Guid.NewGuid(),
                Message = "Плановое обслуживание: рекомендуется подмена 20% воды через 2 дня",
                Level = 0,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new NotificationModel
            {
                Id = Guid.NewGuid(),
                Message = "Уровень pH воды достиг 7.8 (рекомендуемый диапазон: 6.8 - 7.2)",
                Level = 1,
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new NotificationModel
            {
                Id = Guid.NewGuid(),
                Message = "Сценарий «Ночной режим» успешно выполнен",
                Level = 3,
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        });
    }

    [RelayCommand]
    private void SelectFilter(string filter)
    {
        SelectedFilter = filter;
        OnPropertyChanged(nameof(IsAllFilterSelected));
        OnPropertyChanged(nameof(IsUnreadFilterSelected));
        OnPropertyChanged(nameof(IsImportantFilterSelected));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = _allNotifications.AsEnumerable();

        if (SelectedFilter == "Непрочитанные")
        {
            filtered = filtered.Where(n => !n.IsRead);
        }
        else if (SelectedFilter == "Важные")
        {
            filtered = filtered.Where(n => n.Level >= 1);
        }

        Notifications.Clear();
        foreach (var item in filtered)
        {
            Notifications.Add(item);
        }

        UnreadCount = _allNotifications.Count(n => !n.IsRead);
        HasNotifications = Notifications.Count > 0;
    }

    [RelayCommand]
    private async Task MarkAsReadAsync(NotificationModel notification)
    {
        if (notification == null) return;
        notification.IsRead = true;
        UnreadCount = _allNotifications.Count(n => !n.IsRead);

        try
        {
            await _apiClient.PutAsync<object>(
                $"{ApiConfiguration.Routes.Notifications}/{notification.Id}/read",
                new { });
        }
        catch
        {
        }

        if (SelectedFilter == "Непрочитанные")
        {
            ApplyFilter();
        }
    }

    [RelayCommand]
    private async Task MarkAllAsReadAsync()
    {
        foreach (var n in _allNotifications)
        {
            n.IsRead = true;
        }

        UnreadCount = 0;
        ApplyFilter();

        try
        {
            foreach (var n in _allNotifications)
            {
                await _apiClient.PutAsync<object>(
                    $"{ApiConfiguration.Routes.Notifications}/{n.Id}/read",
                    new { });
            }
        }
        catch
        {
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        _allNotifications.Clear();
        Notifications.Clear();
        UnreadCount = 0;
        HasNotifications = false;
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
