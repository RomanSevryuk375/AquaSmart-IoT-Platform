using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class AddControllerViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private string _controllerName = "Контроллер №123";

    [ObservableProperty]
    private string _controllerCode = "AQ-ESP32-9842";

    [ObservableProperty]
    private string _macAddress = "00:1A:2B:3C:4D:5E";

    [ObservableProperty]
    private string _modelType = "ESP32-AquaHub v2";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsManualMode))]
    private bool _isQrMode = true;

    public bool IsManualMode => !IsQrMode;

    [ObservableProperty]
    private bool _isQrScanned = false;

    // View States matching Figma
    [ObservableProperty]
    private bool _isFormVisible = true;

    [ObservableProperty]
    private bool _isConnectingVisible = false;

    [ObservableProperty]
    private bool _isSuccessVisible = false;

    [ObservableProperty]
    private bool _isCancelDialogOpen = false;

    // Created controller summary
    [ObservableProperty]
    private string _createdControllerTitle = "Контроллер №123";

    [ObservableProperty]
    private string _createdControllerMac = "00:1A:2B:3C:4D:5E";

    [ObservableProperty]
    private string _createdControllerStatus = "В сети";

    public AddControllerViewModel(IControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    [RelayCommand]
    private void SwitchToQrMode()
    {
        IsQrMode = true;
    }

    [RelayCommand]
    private void SwitchToManualMode()
    {
        IsQrMode = false;
    }

    [RelayCommand]
    private async Task SimulateQrScanAsync()
    {
        IsQrScanned = true;
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch { }

        await Shell.Current.DisplayAlert("QR-код распознан", "Контроллер: ESP32-AquaHub v2\nMAC: 00:1A:2B:3C:4D:5E\nКод: AQ-ESP32-9842", "OK");
    }

    [RelayCommand]
    private async Task ConnectControllerAsync()
    {
        if (string.IsNullOrWhiteSpace(ControllerName))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите название контроллера", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ControllerCode))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите код контроллера или отсканируйте QR-код", "OK");
            return;
        }

        // Show Fullscreen connecting state
        IsFormVisible = false;
        IsConnectingVisible = true;

        await Task.Delay(1200);

        var controller = new ControllerModel
        {
            Name = ControllerName.Trim(),
            ModelType = ModelType,
            MacAddress = MacAddress,
            IpAddress = "192.168.1." + new Random().Next(100, 200),
            FirmwareVersion = "v2.4.1 (Latest)",
            IsOnline = true,
            LastSeen = DateTime.UtcNow,
            AttachedAquariumName = "Не привязан"
        };

        await _controllerService.AddControllerAsync(controller);

        CreatedControllerTitle = controller.Name;
        CreatedControllerMac = controller.MacAddress;
        CreatedControllerStatus = "В сети";

        IsConnectingVisible = false;
        IsSuccessVisible = true;
    }

    [RelayCommand]
    private void PromptCancel()
    {
        IsCancelDialogOpen = true;
    }

    [RelayCommand]
    private void ResumeEditing()
    {
        IsCancelDialogOpen = false;
    }

    [RelayCommand]
    private async Task ConfirmCancelAsync()
    {
        IsCancelDialogOpen = false;
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ReturnToControllersAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task GoHomeAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/DashboardPage");
    }
}
