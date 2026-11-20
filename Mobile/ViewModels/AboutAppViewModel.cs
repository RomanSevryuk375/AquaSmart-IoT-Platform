using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class AboutAppViewModel : ObservableObject
{
    [RelayCommand]
    private async Task OpenTermsAsync()
    {
        var title = LocalizationService.Instance["About_Terms"];
        var msg = LocalizationService.Instance.IsEnglish
            ? "AquaSmart IoT Platform Terms of Service.\n1. The service is provided as-is.\n2. The user is responsible for the security of their account and devices.\n3. Read the full version at aquasmart.io/terms"
            : "Условия использования платформы AquaSmart IoT Platform.\n1. Сервис предоставляется как есть.\n2. Пользователь несет ответственность за безопасность своего аккаунта и оборудования.\n3. С полной версией можно ознакомиться на aquasmart.io/terms";
        await Shell.Current.DisplayAlert(title, msg, LocalizationService.Instance["Common_Close"]);
    }

    [RelayCommand]
    private async Task OpenPrivacyAsync()
    {
        var title = LocalizationService.Instance["About_Privacy"];
        var msg = LocalizationService.Instance.IsEnglish
            ? "We value your privacy. Telemetry and account data are encrypted using TLS 1.3 / AES-256 and never shared with third parties."
            : "Мы ценим вашу приватность. Данные телеметрии и учетных записей шифруются по стандарту TLS 1.3 / AES-256 и не передаются третьим лицам.";
        await Shell.Current.DisplayAlert(title, msg, LocalizationService.Instance["Common_Close"]);
    }

    [RelayCommand]
    private async Task OpenLicensesAsync()
    {
        var title = LocalizationService.Instance["About_Licenses"];
        var msg = LocalizationService.Instance.IsEnglish
            ? "This app uses open-source libraries:\n• .NET 8 MAUI (MIT License)\n• CommunityToolkit.Mvvm (MIT License)\n• SkiaSharp (MIT License)"
            : "Приложение использует открытые библиотеки:\n• .NET 8 MAUI (MIT License)\n• CommunityToolkit.Mvvm (MIT License)\n• SkiaSharp (MIT License)";
        await Shell.Current.DisplayAlert(title, msg, LocalizationService.Instance["Common_Close"]);
    }

    [RelayCommand]
    private async Task OpenWebsiteAsync()
    {
        try
        {
            await Launcher.OpenAsync(new Uri("https://aquasmart.io"));
        }
        catch
        {
            var title = LocalizationService.Instance["About_Website"];
            var msg = LocalizationService.Instance.IsEnglish
                ? "Official website: https://aquasmart.io"
                : "Официальный веб-сайт: https://aquasmart.io";
            await Shell.Current.DisplayAlert(title, msg, LocalizationService.Instance["Common_Ok"]);
        }
    }

    [RelayCommand]
    private async Task RateAppAsync()
    {
        var title = LocalizationService.Instance["About_Rate"];
        var msg = LocalizationService.Instance.IsEnglish
            ? "Thank you for using AquaSmart! Google Play / App Store ratings will be available in the public release."
            : "Спасибо за интерес к AquaSmart! Оценка в Google Play / App Store будет доступна в финальном релизе.";
        await Shell.Current.DisplayAlert(title, msg, LocalizationService.Instance["Common_Ok"]);
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
