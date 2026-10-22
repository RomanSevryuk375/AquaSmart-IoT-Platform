using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class AboutAppViewModel : ObservableObject
{
    [RelayCommand]
    private async Task OpenTermsAsync()
    {
        await Shell.Current.DisplayAlert("Пользовательское соглашение", "Условия использования платформы AquaSmart IoT Platform.\n1. Сервис предоставляется как есть.\n2. Пользователь несет ответственность за безопасность своего аккаунта и оборудования.\n3. С полной версией можно ознакомиться на aquasmart.io/terms", "Закрыть");
    }

    [RelayCommand]
    private async Task OpenPrivacyAsync()
    {
        await Shell.Current.DisplayAlert("Политика конфиденциальности", "Мы ценим вашу приватность. Данные телеметрии и учетных записей шифруются по стандарту TLS 1.3 / AES-256 и не передаются третьим лицам.", "Закрыть");
    }

    [RelayCommand]
    private async Task OpenLicensesAsync()
    {
        await Shell.Current.DisplayAlert("Лицензии Open Source", "Приложение использует открытые библиотеки:\n• .NET 8 MAUI (MIT License)\n• CommunityToolkit.Mvvm (MIT License)\n• SkiaSharp (MIT License)", "Закрыть");
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
            await Shell.Current.DisplayAlert("Сайт", "Официальный веб-сайт: https://aquasmart.io", "OK");
        }
    }

    [RelayCommand]
    private async Task RateAppAsync()
    {
        await Shell.Current.DisplayAlert("Оценить приложение", "Спасибо за интерес к AquaSmart! Оценка в Google Play / App Store будет доступна в финальном релизе.", "Спасибо!");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
