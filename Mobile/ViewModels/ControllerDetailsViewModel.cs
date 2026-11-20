using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;
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
    private string _otaStatusText = LocalizationService.Instance["OTA_LatestVersion"];

    [ObservableProperty]
    private string _otaStatusColor = "#4CAF50";

    [ObservableProperty]
    private bool _isCheckingOta;

    public string OnlineStatusText => (Controller?.IsOnline ?? true)
        ? LocalizationService.Instance["Controller_Online"]
        : LocalizationService.Instance["Controller_Offline"];

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
                OnPropertyChanged(nameof(OnlineStatusText));
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
        OtaStatusText = LocalizationService.Instance["OTA_Checking"];
        try
        {
            var result = await _controllerService.CheckOtaUpdateAsync(Controller.Id);
            if (result == null)
            {
                OtaStatusText = LocalizationService.Instance["OTA_LatestVersion"];
                OtaStatusColor = "#4CAF50";
                await Shell.Current.DisplayAlert(
                    LocalizationService.Instance.IsEnglish ? "OTA Update" : "OTA Обновление",
                    LocalizationService.Instance.IsEnglish ? "Controller is running the latest version. No active deployment campaigns found." : "Контроллер работает на актуальной версии. Активных кампаний развертывания не найдено.",
                    LocalizationService.Instance["Common_Ok"]);
            }
            else if (result.HasUpdate)
            {
                OtaStatusText = string.Format(LocalizationService.Instance["OTA_UpdateAvailable"], result.Version);
                OtaStatusColor = "#FF9800";
                var proceed = await Shell.Current.DisplayAlert(
                    LocalizationService.Instance.IsEnglish ? "New Firmware Found" : "Найдена новая прошивка",
                    LocalizationService.Instance.IsEnglish ? $"Version {result.Version} available.\nHash: {result.FileHash?[..8]}...\nDownload firmware update image?" : $"Доступна версия {result.Version}.\nХеш: {result.FileHash?[..8]}...\nЗагрузить образ обновления?",
                    LocalizationService.Instance.IsEnglish ? "Download" : "Загрузить",
                    LocalizationService.Instance.IsEnglish ? "Later" : "Позже");

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
                OtaStatusText = LocalizationService.Instance["OTA_LatestVersion"];
                OtaStatusColor = "#4CAF50";
                await Shell.Current.DisplayAlert(
                    LocalizationService.Instance.IsEnglish ? "OTA Update" : "OTA Обновление",
                    LocalizationService.Instance.IsEnglish ? "The controller is running the latest firmware version." : "На контроллере установлена самая свежая версия прошивки.",
                    LocalizationService.Instance["Common_Ok"]);
            }
        }
        catch (Exception ex)
        {
            OtaStatusText = LocalizationService.Instance["OTA_CheckFailed"];
            OtaStatusColor = "#E53935";
            await Shell.Current.DisplayAlert(
                LocalizationService.Instance["Common_Error"],
                LocalizationService.Instance.IsEnglish ? $"Failed to check OTA: {ex.Message}" : $"Не удалось проверить OTA: {ex.Message}",
                LocalizationService.Instance["Common_Ok"]);
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
        await Shell.Current.DisplayAlert(
            LocalizationService.Instance["Common_Success"],
            LocalizationService.Instance.IsEnglish ? "Controller changes saved!" : "Изменения контроллера сохранены!",
            LocalizationService.Instance["Common_Ok"]);
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
