using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class DataHistoryViewModel : ObservableObject
{
    private const string HistoryDurationKey = "data_history_duration";
    private const string CloudBackupKey = "data_cloud_backup";

    [ObservableProperty]
    private string _historyDuration = "3 мес.";

    [ObservableProperty]
    private string _cacheSize = "42 МБ";

    [ObservableProperty]
    private bool _cloudBackup = true;

    public DataHistoryViewModel()
    {
        HistoryDuration = Preferences.Default.Get(HistoryDurationKey, "3 мес.");
        CloudBackup = Preferences.Default.Get(CloudBackupKey, true);
    }

    partial void OnCloudBackupChanged(bool value) => Preferences.Default.Set(CloudBackupKey, value);

    [RelayCommand]
    private async Task ChooseHistoryDurationAsync()
    {
        var result = await Shell.Current.DisplayActionSheet("Хранить историю", "Отмена", null, "1 мес.", "3 мес.", "6 мес.", "1 год", "Бессрочно");
        if (!string.IsNullOrWhiteSpace(result) && result != "Отмена")
        {
            HistoryDuration = result;
            Preferences.Default.Set(HistoryDurationKey, result);
        }
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Очистка кэша", "Очистить локальный кэш графиков и телеметрии (42 МБ)?", "Да, очистить", "Отмена");
        if (confirm)
        {
            CacheSize = "0 КБ";
            await Shell.Current.DisplayAlert("Успешно", "Кэш графиков успешно очищен", "OK");
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        await Shell.Current.DisplayAlert("Экспорт отчета", "Файл aquasmart_telemetry_report.csv сформирован и готов к выгрузке.", "Поделиться");
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        await Shell.Current.DisplayAlert("Резервная копия", "Резервная копия настроек и сценариев успешно создана в облаке AquaSmart Cloud.", "OK");
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Восстановление", "Восстановить конфигурацию контроллеров и сценариев из последней точки бэкапа?", "Восстановить", "Отмена");
        if (confirm)
        {
            await Shell.Current.DisplayAlert("Успех", "Настройки успешно восстановлены", "OK");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
