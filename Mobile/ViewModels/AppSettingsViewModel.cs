using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class AppSettingsViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;

    private const string ThemeKey = "setting_theme";
    private const string LanguageKey = "setting_language";
    private const string RefreshIntervalKey = "setting_refresh_interval";
    private const string EmergencyAlertsKey = "setting_emergency_alerts";
    private const string ConnectionAlertsKey = "setting_connection_alerts";
    private const string ScenarioAlertsKey = "setting_scenario_alerts";
    private const string TrafficSavingKey = "setting_traffic_saving";
    private const string TelegramAlertsKey = "setting_telegram_alerts";

    [ObservableProperty]
    private string _currentTheme = "Светлая";

    [ObservableProperty]
    private string _currentLanguage = "Русский";

    [ObservableProperty]
    private string _refreshInterval = "30 сек.";

    [ObservableProperty]
    private bool _emergencyAlerts = true;

    [ObservableProperty]
    private bool _connectionLossAlerts = true;

    [ObservableProperty]
    private bool _scenarioAlerts = true;

    [ObservableProperty]
    private bool _trafficSaving = true;

    [ObservableProperty]
    private bool _telegramAlerts = true;

    [ObservableProperty]
    private string _cacheSize = "0.0 МБ";

    [ObservableProperty]
    private bool _isClearingCache;

    public AppSettingsViewModel(IIdentityService identityService)
    {
        _identityService = identityService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        CurrentTheme = Preferences.Default.Get(ThemeKey, "Светлая");
        CurrentLanguage = Preferences.Default.Get(LanguageKey, "Русский");
        RefreshInterval = Preferences.Default.Get(RefreshIntervalKey, "30 сек.");
        EmergencyAlerts = Preferences.Default.Get(EmergencyAlertsKey, true);
        ConnectionLossAlerts = Preferences.Default.Get(ConnectionAlertsKey, true);
        ScenarioAlerts = Preferences.Default.Get(ScenarioAlertsKey, true);
        TrafficSaving = Preferences.Default.Get(TrafficSavingKey, true);
        TelegramAlerts = Preferences.Default.Get(TelegramAlertsKey, true);
        CalculateCacheSize();
    }

    private static void ApplyTheme(string theme)
    {
        try
        {
            if (Application.Current == null) return;
            Application.Current.UserAppTheme = theme switch
            {
                "Светлая" => AppTheme.Light,
                "Тёмная" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }
        catch
        {
        }
    }

    partial void OnEmergencyAlertsChanged(bool value) => Preferences.Default.Set(EmergencyAlertsKey, value);
    partial void OnConnectionLossAlertsChanged(bool value) => Preferences.Default.Set(ConnectionAlertsKey, value);
    partial void OnScenarioAlertsChanged(bool value) => Preferences.Default.Set(ScenarioAlertsKey, value);
    partial void OnTrafficSavingChanged(bool value) => Preferences.Default.Set(TrafficSavingKey, value);
    partial void OnTelegramAlertsChanged(bool value) => Preferences.Default.Set(TelegramAlertsKey, value);

    [RelayCommand]
    private async Task LinkTelegramAsync()
    {
        try
        {
            var link = await _identityService.GenerateTelegramLinkTokenAsync();
            if (!string.IsNullOrWhiteSpace(link))
            {
                await Launcher.OpenAsync(new Uri(link));
            }
            else
            {
                await Shell.Current.DisplayAlert("Telegram", "Не удалось создать ссылку для привязки. Проверьте подключение к серверу.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ChooseThemeAsync()
    {
        var result = await Shell.Current.DisplayActionSheet("Тема интерфейса", "Отмена", null, "Светлая", "Тёмная", "Системная");
        if (!string.IsNullOrWhiteSpace(result) && result != "Отмена")
        {
            CurrentTheme = result;
            Preferences.Default.Set(ThemeKey, result);
            MainThread.BeginInvokeOnMainThread(() => ApplyTheme(result));
        }
    }

    [RelayCommand]
    private async Task ChooseLanguageAsync()
    {
        var result = await Shell.Current.DisplayActionSheet("Язык приложения", "Отмена", null, "Русский", "English");
        if (!string.IsNullOrWhiteSpace(result) && result != "Отмена")
        {
            CurrentLanguage = result;
            Preferences.Default.Set(LanguageKey, result);
        }
    }

    [RelayCommand]
    private async Task ChooseRefreshIntervalAsync()
    {
        var result = await Shell.Current.DisplayActionSheet("Обновление экранов", "Отмена", null, "10 сек.", "30 сек.", "60 сек.", "2 мин.");
        if (!string.IsNullOrWhiteSpace(result) && result != "Отмена")
        {
            RefreshInterval = result;
            Preferences.Default.Set(RefreshIntervalKey, result);
            await Shell.Current.DisplayAlert("Обновление данных", $"Периодичность автообновления установлена на {result}", "OK");
        }
    }

    public static TimeSpan GetUiRefreshInterval()
    {
        var str = Preferences.Default.Get(RefreshIntervalKey, "30 сек.");
        return str switch
        {
            "10 сек." => TimeSpan.FromSeconds(10),
            "30 сек." => TimeSpan.FromSeconds(30),
            "60 сек." => TimeSpan.FromSeconds(60),
            "2 мин." => TimeSpan.FromMinutes(2),
            _ => TimeSpan.FromSeconds(30)
        };
    }

    public void CalculateCacheSize()
    {
        long totalBytes = 0;
        try
        {
            var cacheDir = FileSystem.CacheDirectory;
            if (Directory.Exists(cacheDir))
            {
                var dirInfo = new DirectoryInfo(cacheDir);
                foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    totalBytes += file.Length;
                }
            }

            var appDataDir = FileSystem.AppDataDirectory;
            if (Directory.Exists(appDataDir))
            {
                var tempDir = Path.Combine(appDataDir, "temp");
                if (Directory.Exists(tempDir))
                {
                    var dirInfo = new DirectoryInfo(tempDir);
                    foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        totalBytes += file.Length;
                    }
                }
            }
        }
        catch
        {
        }

        if (totalBytes < 1024)
        {
            CacheSize = $"{totalBytes} Б";
        }
        else if (totalBytes < 1024 * 1024)
        {
            CacheSize = $"{(totalBytes / 1024.0):F1} КБ";
        }
        else
        {
            CacheSize = $"{(totalBytes / (1024.0 * 1024.0)):F1} МБ";
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Очистка кэша",
            "Удалить временные файлы, сохранённые миниатюры и кэш ответов сервера?",
            "Очистить",
            "Отмена");

        if (!confirm) return;

        IsClearingCache = true;
        try
        {
            var cacheDir = FileSystem.CacheDirectory;
            if (Directory.Exists(cacheDir))
            {
                var dirInfo = new DirectoryInfo(cacheDir);
                foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    try { file.Delete(); } catch { }
                }
                foreach (var subDir in dirInfo.EnumerateDirectories())
                {
                    try { subDir.Delete(true); } catch { }
                }
            }

            CalculateCacheSize();
            await Shell.Current.DisplayAlert("Успех", "Локальный кэш приложения успешно очищен.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось очистить кэш: {ex.Message}", "OK");
        }
        finally
        {
            IsClearingCache = false;
        }
    }

    [RelayCommand]
    private async Task OpenNotificationsAsync()
    {
        await Shell.Current.GoToAsync("NotificationsPage");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
