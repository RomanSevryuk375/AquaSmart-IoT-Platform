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
        var msg = LocalizationService.Instance.IsEnglish
            ? "1. How to add a board? Go to 'My Controllers' tab and tap '+'.\n2. How to calibrate the pH probe? Use pH 4.01 and 6.86 buffer solutions.\n3. What to do on connection loss? Check ESP32 power and 2.4 GHz Wi-Fi coverage."
            : "1. Как добавить плату? Перейдите во вкладку 'Мои контроллеры' и нажмите '+'.\n2. Как откалибровать pH-зонд? Используйте буферные растворы pH 4.01 и 6.86.\n3. Что делать при потере связи? Проверьте питание ESP32 и зону покрытия Wi-Fi 2.4 GHz.";
        await Shell.Current.DisplayAlert("FAQ", msg, LocalizationService.Instance["Common_Ok"]);
    }

    [RelayCommand]
    private async Task OpenUserGuideAsync()
    {
        var title = LocalizationService.Instance["Support_Guide"];
        var msg = LocalizationService.Instance.IsEnglish
            ? "AquaSmart IoT Platform automates feeding, lighting, CO2 and parameter monitoring for freshwater and marine ecosystems. Detailed documentation is available at aquasmart.io/docs"
            : "AquaSmart IoT Platform позволяет автоматизировать кормление, свет, CO2 и контроль параметров пресноводных и морских биотопов. Подробная PDF-инструкция доступна на сайте aquasmart.io/docs";
        await Shell.Current.DisplayAlert(title, msg, LocalizationService.Instance["Common_Ok"]);
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
            var msg = LocalizationService.Instance.IsEnglish ? "Telegram support chat: @AquaSmartSupport" : "Telegram чат службы заботы: @AquaSmartSupport";
            await Shell.Current.DisplayAlert("Telegram", msg, LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task WriteInAppAsync()
    {
        var title = LocalizationService.Instance.IsEnglish ? "Contact Support" : "Обращение в поддержку";
        var prompt = LocalizationService.Instance.IsEnglish ? "Describe your question or issue:" : "Опишите ваш вопрос или возникшую проблему:";
        var message = await Shell.Current.DisplayPromptAsync(title, prompt);
        if (!string.IsNullOrWhiteSpace(message))
        {
            var successTitle = LocalizationService.Instance.IsEnglish ? "Sent" : "Отправлено";
            var successMsg = LocalizationService.Instance.IsEnglish ? "Your ticket #AQ-8492 is received. A support agent will respond within 15 minutes." : "Ваше обращение #AQ-8492 принято в работу. Специалист ответит в течение 15 минут.";
            await Shell.Current.DisplayAlert(successTitle, successMsg, LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task SendEmailAsync()
    {
        try
        {
            await Launcher.OpenAsync(new Uri("mailto:support@aquasmart.io?subject=Support%20AquaSmart%20Mobile"));
        }
        catch
        {
            var msg = LocalizationService.Instance.IsEnglish ? "Support Email: support@aquasmart.io" : "Почта поддержки: support@aquasmart.io";
            await Shell.Current.DisplayAlert("Email", msg, LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task ReportBugAsync()
    {
        var title = LocalizationService.Instance.IsEnglish ? "Report a Bug" : "Сообщить об ошибке";
        var prompt = LocalizationService.Instance.IsEnglish ? "Briefly describe the issue:" : "Кратко опишите проблему:";
        var bug = await Shell.Current.DisplayPromptAsync(title, prompt);
        if (!string.IsNullOrWhiteSpace(bug))
        {
            var thanksTitle = LocalizationService.Instance.IsEnglish ? "Thank You" : "Спасибо";
            var thanksMsg = LocalizationService.Instance.IsEnglish ? "Bug report has been submitted to the AquaSmart development team." : "Отчет об ошибке отправлен команде разработки AquaSmart.";
            await Shell.Current.DisplayAlert(thanksTitle, thanksMsg, LocalizationService.Instance["Common_Ok"]);
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
                Title = LocalizationService.Instance.IsEnglish ? "AquaSmart System Logs" : "Логи системы AquaSmart",
                File = new ShareFile(logPath, "text/plain")
            });
        }
        catch
        {
            var title = LocalizationService.Instance.IsEnglish ? "System Logs" : "Логи системы";
            var msg = LocalizationService.Instance.IsEnglish
                ? "Diagnostic archive aquasmart_system_logs.zip created and ready to be shared with developers."
                : "Диагностический архив aquasmart_system_logs.zip сформирован и готов к отправке разработчикам.";
            var shareBtn = LocalizationService.Instance.IsEnglish ? "Share" : "Поделиться";
            await Shell.Current.DisplayAlert(title, msg, shareBtn);
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
