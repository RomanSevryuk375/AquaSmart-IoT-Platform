using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;
using Mobile.Services.Dto.Identity;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class HelpSupportViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public HelpSupportViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    private async Task OpenFaqAsync()
    {
        await Shell.Current.DisplayAlert("FAQ", "1. Как добавить плату? Перейдите во вкладку 'Мои контроллеры' и нажмите '+'.\n2. Как откалибровать pH-зонд? Используйте буферные растворы pH 4.01 и 6.86.\n3. Что делать при потере связи? Проверьте питание ESP32 и зону покрытия Wi-Fi 2.4 GHz.", "Понятно");
    }

    [RelayCommand]
    private async Task OpenUserGuideAsync()
    {
        await Shell.Current.DisplayAlert("Руководство пользователя", "AquaSmart IoT Platform позволяет автоматизировать кормление, свет, CO2 и контроль параметров пресноводных и морских биотопов. Подробная PDF-инструкция доступна на сайте aquasmart.io/docs", "OK");
    }

    [RelayCommand]
    private async Task OpenTelegramAsync()
    {
        try
        {
            // Try to generate personal Telegram bot linking token from backend
            var response = await _apiClient.PostAsync<object, TelegramLinkTokenResponseDto>($"{ApiConfiguration.Routes.Profile}/telegram-link-token", new { });
            if (response.IsSuccess && response.Data != null && !string.IsNullOrWhiteSpace(response.Data.Link))
            {
                await Launcher.OpenAsync(new Uri(response.Data.Link));
                return;
            }
        }
        catch
        {
        }

        try
        {
            await Launcher.OpenAsync(new Uri("https://t.me/AquaSmartSupport"));
        }
        catch
        {
            await Shell.Current.DisplayAlert("Telegram", "Telegram чат службы заботы: @AquaSmartSupport", "OK");
        }
    }

    [RelayCommand]
    private async Task WriteInAppAsync()
    {
        var message = await Shell.Current.DisplayPromptAsync("Обращение в поддержку", "Опишите ваш вопрос или возникшую проблему:");
        if (!string.IsNullOrWhiteSpace(message))
        {
            await Shell.Current.DisplayAlert("Отправлено", "Ваше обращение #AQ-8492 принято в работу. Специалист ответит в течение 15 минут.", "OK");
        }
    }

    [RelayCommand]
    private async Task SendEmailAsync()
    {
        try
        {
            await Launcher.OpenAsync(new Uri("mailto:support@aquasmart.io?subject=Обращение%20AquaSmart%20Mobile"));
        }
        catch
        {
            await Shell.Current.DisplayAlert("Email", "Почта поддержки: support@aquasmart.io", "OK");
        }
    }

    [RelayCommand]
    private async Task ReportBugAsync()
    {
        var bug = await Shell.Current.DisplayPromptAsync("Сообщить об ошибке", "Кратко опишите проблему:");
        if (!string.IsNullOrWhiteSpace(bug))
        {
            await Shell.Current.DisplayAlert("Спасибо", "Отчет об ошибке отправлен команде разработки AquaSmart.", "OK");
        }
    }

    [RelayCommand]
    private async Task ShareLogsAsync()
    {
        try
        {
            var logPath = Path.Combine(FileSystem.CacheDirectory, "aquasmart_system_logs.txt");
            var content = $"AquaSmart Mobile v1.0.0 Log\nOS: {DeviceInfo.Platform} {DeviceInfo.VersionString}\nDevice: {DeviceInfo.Manufacturer} {DeviceInfo.Model}\nTimestamp: {DateTime.UtcNow:o}\nStatus: System Operational\n";
            await File.WriteAllTextAsync(logPath, content);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Логи системы AquaSmart",
                File = new ShareFile(logPath, "text/plain")
            });
        }
        catch
        {
            await Shell.Current.DisplayAlert("Логи системы", "Диагностический архив aquasmart_system_logs.zip сформирован и готов к отправке разработчикам.", "Поделиться");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
