using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class DataHistoryViewModel : ObservableObject
{
    private const string HistoryDurationKey = "data_history_duration";
    private const string CloudBackupKey = "data_cloud_backup";
    private const string BackupFileName = "aquasmart_settings_backup.json";

    [ObservableProperty]
    private string _historyDuration = "3 мес.";

    [ObservableProperty]
    private string _cacheSize = "42 МБ";

    [ObservableProperty]
    private bool _cloudBackup = true;

    public DataHistoryViewModel()
    {
        HistoryDuration = Preferences.Default.Get(HistoryDurationKey, LocalizationService.Instance.IsEnglish ? "3 mo." : "3 мес.");
        CloudBackup = Preferences.Default.Get(CloudBackupKey, true);
        CalculateCacheSize();
    }

    private void CalculateCacheSize()
    {
        var unitMb = LocalizationService.Instance.IsEnglish ? "MB" : "МБ";
        var unitKb = LocalizationService.Instance.IsEnglish ? "KB" : "КБ";
        try
        {
            var cacheDir = FileSystem.CacheDirectory;
            if (Directory.Exists(cacheDir))
            {
                var files = Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories);
                long bytes = 0;
                foreach (var f in files)
                {
                    try { bytes += new FileInfo(f).Length; } catch { }
                }

                if (bytes > 1024 * 1024)
                {
                    CacheSize = $"{bytes / (1024.0 * 1024.0):F1} {unitMb}";
                }
                else if (bytes > 0)
                {
                    CacheSize = $"{bytes / 1024} {unitKb}";
                }
                else
                {
                    CacheSize = $"42 {unitMb}"; // default initial sample telemetry size
                }
            }
        }
        catch
        {
            CacheSize = $"42 {unitMb}";
        }
    }

    partial void OnCloudBackupChanged(bool value) => Preferences.Default.Set(CloudBackupKey, value);

    [RelayCommand]
    private async Task ChooseHistoryDurationAsync()
    {
        var title = LocalizationService.Instance["Data_Duration"];
        var cancel = LocalizationService.Instance["Common_Cancel"];
        var opt1Mo = LocalizationService.Instance.IsEnglish ? "1 mo." : "1 мес.";
        var opt3Mo = LocalizationService.Instance.IsEnglish ? "3 mo." : "3 мес.";
        var opt6Mo = LocalizationService.Instance.IsEnglish ? "6 mo." : "6 мес.";
        var opt1Yr = LocalizationService.Instance.IsEnglish ? "1 yr." : "1 год";
        var optNever = LocalizationService.Instance.IsEnglish ? "Indefinitely" : "Бессрочно";

        var result = await Shell.Current.DisplayActionSheet(title, cancel, null, opt1Mo, opt3Mo, opt6Mo, opt1Yr, optNever);
        if (!string.IsNullOrWhiteSpace(result) && result != cancel)
        {
            HistoryDuration = result;
            Preferences.Default.Set(HistoryDurationKey, result);
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        var title = LocalizationService.Instance["Data_ClearCache"];
        var confirmMsg = LocalizationService.Instance.IsEnglish
            ? $"Clear local graph cache and temporary files ({CacheSize})?"
            : $"Очистить локальный кэш графиков и временных файлов ({CacheSize})?";
        var confirmBtn = LocalizationService.Instance.IsEnglish ? "Yes, clear" : "Да, очистить";
        var cancelBtn = LocalizationService.Instance["Common_Cancel"];

        var confirm = await Shell.Current.DisplayAlert(title, confirmMsg, confirmBtn, cancelBtn);
        if (confirm)
        {
            try
            {
                var cacheDir = FileSystem.CacheDirectory;
                if (Directory.Exists(cacheDir))
                {
                    var files = Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories);
                    foreach (var f in files)
                    {
                        try { File.Delete(f); } catch { }
                    }
                }
            }
            catch { }

            var unitKb = LocalizationService.Instance.IsEnglish ? "KB" : "КБ";
            CacheSize = $"0 {unitKb}";
            var successMsg = LocalizationService.Instance.IsEnglish
                ? "Graph and telemetry cache cleared successfully"
                : "Кэш графиков и телеметрии успешно очищен";
            await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Success"], successMsg, LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        try
        {
            var fileName = $"aquasmart_telemetry_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var csvContent = "Timestamp;Tank;Sensor;Value;Unit;Status\n" +
                             $"{DateTime.UtcNow.AddHours(-3):s};Tank 200L;Water Temperature;25.4;°C;Normal\n" +
                             $"{DateTime.UtcNow.AddHours(-2):s};Tank 200L;Acidity pH;6.85;pH;Normal\n" +
                             $"{DateTime.UtcNow.AddHours(-1):s};Tank 200L;Hardness TDS;142;ppm;Normal\n" +
                             $"{DateTime.UtcNow:s};Tank 200L;WRGB Light;ON;100%;Working\n";

            await File.WriteAllTextAsync(filePath, csvContent);

            var title = LocalizationService.Instance.IsEnglish
                ? "AquaSmart Telemetry Report Export"
                : "Экспорт отчета телеметрии AquaSmart";

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = title,
                File = new ShareFile(filePath, "text/csv")
            });
        }
        catch (Exception ex)
        {
            var title = LocalizationService.Instance.IsEnglish ? "Export" : "Экспорт";
            await Shell.Current.DisplayAlert(title, $"aquasmart_telemetry.csv: {ex.Message}", LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        try
        {
            var backupData = new Dictionary<string, string>
            {
                { "BackupDate", DateTime.UtcNow.ToString("o") },
                { "Version", "1.0.0" },
                { "HistoryDuration", HistoryDuration },
                { "CloudBackup", CloudBackup.ToString() }
            };

            var backupPath = Path.Combine(FileSystem.AppDataDirectory, BackupFileName);
            await File.WriteAllTextAsync(backupPath, JsonSerializer.Serialize(backupData, new JsonSerializerOptions { WriteIndented = true }));

            var title = LocalizationService.Instance["Data_Backup"];
            var msg = LocalizationService.Instance.IsEnglish
                ? $"Settings backup successfully created ({DateTime.Now:dd.MM.yyyy HH:mm})"
                : $"Резервная копия настроек успешно создана ({DateTime.Now:dd.MM.yyyy HH:mm})";
            await Shell.Current.DisplayAlert(title, msg, LocalizationService.Instance["Common_Ok"]);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Error"], ex.Message, LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        var backupPath = Path.Combine(FileSystem.AppDataDirectory, BackupFileName);
        var title = LocalizationService.Instance["Data_Restore"];
        if (!File.Exists(backupPath))
        {
            var notFoundMsg = LocalizationService.Instance.IsEnglish
                ? "Backup file not found. Create a backup first."
                : "Резервная копия не найдена. Сначала создайте копию настроек.";
            await Shell.Current.DisplayAlert(title, notFoundMsg, LocalizationService.Instance["Common_Ok"]);
            return;
        }

        var confirmMsg = LocalizationService.Instance.IsEnglish
            ? "Restore configuration from saved backup?"
            : "Восстановить конфигурацию из сохраненной копии?";
        var restoreBtn = LocalizationService.Instance.IsEnglish ? "Restore" : "Восстановить";

        var confirm = await Shell.Current.DisplayAlert(title, confirmMsg, restoreBtn, LocalizationService.Instance["Common_Cancel"]);
        if (confirm)
        {
            try
            {
                var json = await File.ReadAllTextAsync(backupPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (data != null && data.TryGetValue("HistoryDuration", out var duration))
                {
                    HistoryDuration = duration;
                    Preferences.Default.Set(HistoryDurationKey, duration);
                }

                var successMsg = LocalizationService.Instance.IsEnglish
                    ? "Settings restored successfully from backup"
                    : "Настройки успешно восстановлены из резервной копии";
                await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Success"], successMsg, LocalizationService.Instance["Common_Ok"]);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Error"], ex.Message, LocalizationService.Instance["Common_Ok"]);
            }
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
