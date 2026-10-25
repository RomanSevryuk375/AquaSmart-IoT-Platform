using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class AppSettingsViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;
    private readonly ILocalizationService _localizationService;

    private const string ThemeKey = "setting_theme";
    private const string LanguageKey = "setting_language";
    private const string RefreshIntervalKey = "setting_refresh_interval";
    private const string EmergencyAlertsKey = "setting_emergency_alerts";
    private const string ConnectionAlertsKey = "setting_connection_alerts";
    private const string ScenarioAlertsKey = "setting_scenario_alerts";
    private const string TrafficSavingKey = "setting_traffic_saving";
    private const string TelegramAlertsKey = "setting_telegram_alerts";

    [ObservableProperty]
    private string _currentTheme = "Системная";

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

    public AppSettingsViewModel(IIdentityService identityService, ILocalizationService localizationService)
    {
        _identityService = identityService;
        _localizationService = localizationService;
        _localizationService.LanguageChanged += (s, e) =>
        {
            UpdateLocalizedTheme();
            CalculateCacheSize();
        };
        LoadSettings();
    }

    private void LoadSettings()
    {
        UpdateLocalizedTheme();
        CurrentLanguage = _localizationService.CurrentLanguage;
        RefreshInterval = Preferences.Default.Get(RefreshIntervalKey, "30 сек.");
        EmergencyAlerts = Preferences.Default.Get(EmergencyAlertsKey, true);
        ConnectionLossAlerts = Preferences.Default.Get(ConnectionAlertsKey, true);
        ScenarioAlerts = Preferences.Default.Get(ScenarioAlertsKey, true);
        TrafficSaving = Preferences.Default.Get(TrafficSavingKey, true);
        TelegramAlerts = Preferences.Default.Get(TelegramAlertsKey, true);
        CalculateCacheSize();
    }

    private void UpdateLocalizedTheme()
    {
        var savedTheme = Preferences.Default.Get(ThemeKey, "system");
        CurrentTheme = savedTheme?.ToLowerInvariant() switch
        {
            "light" or "светлая" => _localizationService.GetString("Theme_Light"),
            "dark" or "тёмная" => _localizationService.GetString("Theme_Dark"),
            _ => _localizationService.GetString("Theme_System")
        };
    }

    private static void ApplyTheme(string theme)
    {
        try
        {
            if (Application.Current == null) return;
            Application.Current.UserAppTheme = theme?.ToLowerInvariant() switch
            {
                "light" or "светлая" => AppTheme.Light,
                "dark" or "тёмная" => AppTheme.Dark,
                _ => AppTheme.Unspecified // Default is OS system theme
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
        var title = _localizationService.GetString("Settings_Theme");
        var cancel = _localizationService.GetString("Common_Cancel");
        var light = _localizationService.GetString("Theme_Light");
        var dark = _localizationService.GetString("Theme_Dark");
        var system = _localizationService.GetString("Theme_System");

        var result = await Shell.Current.DisplayActionSheet(title, cancel, null, light, dark, system);
        if (!string.IsNullOrWhiteSpace(result) && result != cancel && result != "Отмена")
        {
            string canonical;
            if (result == dark || result.Equals("Dark", StringComparison.OrdinalIgnoreCase) || result.Equals("Тёмная", StringComparison.OrdinalIgnoreCase))
            {
                canonical = "dark";
                CurrentTheme = dark;
            }
            else if (result == light || result.Equals("Light", StringComparison.OrdinalIgnoreCase) || result.Equals("Светлая", StringComparison.OrdinalIgnoreCase))
            {
                canonical = "light";
                CurrentTheme = light;
            }
            else
            {
                canonical = "system";
                CurrentTheme = system;
            }

            Preferences.Default.Set(ThemeKey, canonical);
            MainThread.BeginInvokeOnMainThread(() => ApplyTheme(canonical));
        }
    }

    [RelayCommand]
    private async Task ChooseLanguageAsync()
    {
        var title = _localizationService.GetString("Settings_Language");
        var cancel = _localizationService.GetString("Common_Cancel");
        var result = await Shell.Current.DisplayActionSheet(title, cancel, null, "Русский", "English");
        if (!string.IsNullOrWhiteSpace(result) && result != cancel && result != "Отмена")
        {
            CurrentLanguage = result;
            _localizationService.SetLanguage(result);
            UpdateLocalizedTheme();
            CalculateCacheSize();
        }
    }

    [RelayCommand]
    private async Task ChooseRefreshIntervalAsync()
    {
        var cancel = _localizationService.GetString("Common_Cancel");
        var result = await Shell.Current.DisplayActionSheet(
            _localizationService.GetString("Settings_RefreshInterval"),
            cancel,
            null,
            "10 сек.", "30 сек.", "60 сек.", "2 мин.");
        if (!string.IsNullOrWhiteSpace(result) && result != cancel && result != "Отмена")
        {
            RefreshInterval = result;
            Preferences.Default.Set(RefreshIntervalKey, result);
            var title = _localizationService.IsEnglish ? "Auto-Refresh" : "Обновление данных";
            var msg = _localizationService.IsEnglish ? $"Refresh rate set to {result}" : $"Периодичность автообновления установлена на {result}";
            await Shell.Current.DisplayAlert(title, msg, _localizationService.GetString("Common_Ok"));
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

        var isEn = _localizationService.IsEnglish;
        if (totalBytes < 1024)
        {
            CacheSize = isEn ? $"{totalBytes} B" : $"{totalBytes} Б";
        }
        else if (totalBytes < 1024 * 1024)
        {
            CacheSize = isEn ? $"{(totalBytes / 1024.0):F1} KB" : $"{(totalBytes / 1024.0):F1} КБ";
        }
        else
        {
            CacheSize = isEn ? $"{(totalBytes / (1024.0 * 1024.0)):F1} MB" : $"{(totalBytes / (1024.0 * 1024.0)):F1} МБ";
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        var title = _localizationService.GetString("Settings_LocalCache");
        var message = _localizationService.IsEnglish
            ? "Delete temporary files, cached thumbnails and server responses?"
            : "Удалить временные файлы, сохранённые миниатюры и кэш ответов сервера?";
        var clearBtn = _localizationService.GetString("Settings_Clear");
        var cancelBtn = _localizationService.GetString("Common_Cancel");

        var confirm = await Shell.Current.DisplayAlert(title, message, clearBtn, cancelBtn);

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
            var successTitle = _localizationService.GetString("Common_Success");
            var successMsg = _localizationService.IsEnglish
                ? "Local app cache has been successfully cleared."
                : "Локальный кэш приложения успешно очищен.";
            await Shell.Current.DisplayAlert(successTitle, successMsg, _localizationService.GetString("Common_Ok"));
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService.GetString("Common_Error");
            var errorMsg = _localizationService.IsEnglish
                ? $"Failed to clear cache: {ex.Message}"
                : $"Не удалось очистить кэш: {ex.Message}";
            await Shell.Current.DisplayAlert(errorTitle, errorMsg, _localizationService.GetString("Common_Ok"));
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
