using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

[QueryProperty(nameof(ControllerIdString), "controllerId")]
public partial class ControllerDetailsViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private string? _controllerIdString;

    [ObservableProperty]
    private ControllerModel? _controller;

    [ObservableProperty]
    private string _controllerCode = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _macAddress = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private bool _isDeleteDialogVisible;

    [ObservableProperty]
    private string _firmwareVersion = "v1.0.0";

    [ObservableProperty]
    private string _otaStatusText = "Установлена актуальная версия";

    [ObservableProperty]
    private string _otaStatusColor = "#4CAF50";

    [ObservableProperty]
    private bool _isCheckingOta;

    public ControllerDetailsViewModel(IControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    async partial void OnControllerIdStringChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
        {
            Controller = await _controllerService.GetControllerByIdAsync(id);
            if (Controller != null)
            {
                Name = Controller.Name;
                MacAddress = Controller.MacAddress;
                ControllerCode = "AQ-" + Controller.Id.ToString()[..8].ToUpperInvariant();
                IsEnabled = Controller.IsOnline;
                if (!string.IsNullOrWhiteSpace(Controller.FirmwareVersion) && Controller.FirmwareVersion != "—")
                {
                    FirmwareVersion = Controller.FirmwareVersion;
                }
            }
        }
    }

    [RelayCommand]
    private async Task CheckOtaUpdateAsync()
    {
        if (Controller == null) return;
        IsCheckingOta = true;
        OtaStatusText = "Проверка обновлений...";
        try
        {
            var result = await _controllerService.CheckOtaUpdateAsync(Controller.Id);
            if (result == null)
            {
                OtaStatusText = "Актуальная версия ПО";
                OtaStatusColor = "#4CAF50";
                await Shell.Current.DisplayAlert("OTA Обновление", "Контроллер работает на актуальной версии. Активных кампаний развертывания не найдено.", "OK");
            }
            else if (result.HasUpdate)
            {
                OtaStatusText = $"Доступно: {result.Version}";
                OtaStatusColor = "#FF9800";
                var proceed = await Shell.Current.DisplayAlert(
                    "Найдена новая прошивка",
                    $"Доступна версия {result.Version}.\nХеш: {result.FileHash?[..8]}...\nЗагрузить образ обновления?",
                    "Загрузить",
                    "Позже");

                if (proceed && !string.IsNullOrWhiteSpace(result.DownloadUrl))
                {
                    if (Uri.TryCreate(result.DownloadUrl, UriKind.Absolute, out var uri))
                    {
                        await Launcher.Default.OpenAsync(uri);
                    }
                }
            }
            else
            {
                OtaStatusText = "Установлена актуальная версия";
                OtaStatusColor = "#4CAF50";
                await Shell.Current.DisplayAlert("OTA Обновление", "На контроллере установлена самая свежая версия прошивки.", "OK");
            }
        }
        catch (Exception ex)
        {
            OtaStatusText = "Ошибка проверки";
            OtaStatusColor = "#E53935";
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось проверить OTA: {ex.Message}", "OK");
        }
        finally
        {
            IsCheckingOta = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Controller != null)
        {
            Controller.Name = Name;
            Controller.MacAddress = MacAddress;
            Controller.IsOnline = IsEnabled;
        }
        await Shell.Current.DisplayAlert("Успех", "Изменения контроллера сохранены!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void PromptDelete()
    {
        IsDeleteDialogVisible = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteDialogVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        IsDeleteDialogVisible = false;
        if (Controller != null)
        {
            await _controllerService.DeleteControllerAsync(Controller.Id);
        }
        await Shell.Current.GoToAsync("..");
    }
}
