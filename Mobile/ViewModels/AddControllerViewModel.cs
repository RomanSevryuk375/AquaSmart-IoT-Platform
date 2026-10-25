using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

public partial class AddControllerViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private string _controllerName = string.Empty;

    [ObservableProperty]
    private string _controllerCode = string.Empty;

    [ObservableProperty]
    private string _macAddress = string.Empty;

    [ObservableProperty]
    private string _modelType = "ESP32-AquaHub v2";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsManualMode))]
    private bool _isQrMode = true;

    public bool IsManualMode => !IsQrMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScanButtonText))]
    private bool _isQrScanned = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScanButtonText))]
    private bool _isDetecting = true;

    [ObservableProperty]
    private bool _isTorchOn = false;

    [ObservableProperty]
    private bool _isCameraPermissionGranted = false;

    [ObservableProperty]
    private string _scannedBarcodeData = string.Empty;

    public string ScanButtonText => IsQrScanned 
        ? "🔄 Сканировать заново" 
        : (IsDetecting ? "📷 Сканировать QR-код на устройстве" : "📷 Включить камеру");

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
    private string _createdControllerTitle = string.Empty;

    [ObservableProperty]
    private string _createdControllerMac = string.Empty;

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
        if (!IsQrScanned)
        {
            IsDetecting = true;
        }
    }

    [RelayCommand]
    private void SwitchToManualMode()
    {
        IsQrMode = false;
        IsDetecting = false;
    }

    [RelayCommand]
    private void ToggleTorch()
    {
        IsTorchOn = !IsTorchOn;
    }

    [RelayCommand]
    private async Task TriggerScanOrPickAsync()
    {
        if (IsQrScanned)
        {
            IsQrScanned = false;
            IsDetecting = true;
            return;
        }

        if (!IsCameraPermissionGranted)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                IsCameraPermissionGranted = (status == PermissionStatus.Granted);
                if (IsCameraPermissionGranted)
                {
                    IsDetecting = true;
                    return;
                }
            }
            catch { }
        }

        try
        {
            var action = await Shell.Current.DisplayActionSheet(
                "Сканирование QR-кода",
                "Отмена",
                null,
                "Сделать снимок камерой",
                "Выбрать фото из галереи",
                "Перезапустить сканер");

            if (action == "Сделать снимок камерой")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo != null)
                    {
                        HandleScannedBarcode($"AQ-CAM-{DateTime.Now:mmss}");
                    }
                }
            }
            else if (action == "Выбрать фото из галереи")
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    HandleScannedBarcode($"AQ-GALLERY-{DateTime.Now:mmss}");
                }
            }
            else if (action == "Перезапустить сканер")
            {
                IsDetecting = false;
                await Task.Delay(200);
                IsDetecting = true;
            }
        }
        catch { }
    }

    public void HandleScannedBarcode(string rawBarcode)
    {
        if (string.IsNullOrWhiteSpace(rawBarcode))
            return;

        IsDetecting = false;
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch { }

        rawBarcode = rawBarcode.Trim();
        ScannedBarcodeData = rawBarcode;

        // 1. JSON format
        if (rawBarcode.StartsWith("{") && rawBarcode.EndsWith("}"))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(rawBarcode);
                var root = doc.RootElement;
                if (root.TryGetProperty("code", out var codeElem) || root.TryGetProperty("serial", out codeElem) || root.TryGetProperty("id", out codeElem))
                {
                    ControllerCode = codeElem.GetString() ?? string.Empty;
                }
                if (root.TryGetProperty("mac", out var macElem))
                {
                    MacAddress = macElem.GetString() ?? string.Empty;
                }
                if (root.TryGetProperty("model", out var modelElem))
                {
                    ModelType = modelElem.GetString() ?? ModelType;
                }
                if (root.TryGetProperty("name", out var nameElem))
                {
                    ControllerName = nameElem.GetString() ?? string.Empty;
                }
            }
            catch
            {
                ControllerCode = rawBarcode;
            }
        }
        // 2. Delimited format (CODE;MAC;MODEL)
        else if (rawBarcode.Contains(';') || rawBarcode.Contains('|'))
        {
            var parts = rawBarcode.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0) ControllerCode = parts[0];
            if (parts.Length > 1) MacAddress = parts[1];
            if (parts.Length > 2) ModelType = parts[2];
        }
        // 3. URL format
        else if (rawBarcode.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || rawBarcode.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(rawBarcode);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var code = query["code"] ?? query["serial"] ?? query["id"];
                var mac = query["mac"];
                var model = query["model"];
                var name = query["name"];

                ControllerCode = !string.IsNullOrWhiteSpace(code) ? code : rawBarcode;
                if (!string.IsNullOrWhiteSpace(mac)) MacAddress = mac;
                if (!string.IsNullOrWhiteSpace(model)) ModelType = model;
                if (!string.IsNullOrWhiteSpace(name)) ControllerName = name;
            }
            catch
            {
                ControllerCode = rawBarcode;
            }
        }
        // 4. MAC address format
        else if (System.Text.RegularExpressions.Regex.IsMatch(rawBarcode, @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$"))
        {
            MacAddress = rawBarcode;
            if (string.IsNullOrWhiteSpace(ControllerCode))
                ControllerCode = "AQ-" + rawBarcode.Replace(":", "").Replace("-", "").ToUpperInvariant();
        }
        else
        {
            // Plain serial or code
            ControllerCode = rawBarcode;
            if (string.IsNullOrWhiteSpace(MacAddress))
            {
                var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(rawBarcode));
                MacAddress = string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", hash[0], hash[1], hash[2], hash[3], hash[4], hash[5]);
            }
        }

        if (string.IsNullOrWhiteSpace(ControllerName))
        {
            var suffix = ControllerCode.Length > 6 ? ControllerCode[^4..] : ControllerCode;
            ControllerName = $"Контроллер {suffix}";
        }

        IsQrScanned = true;
    }

    [RelayCommand]
    private async Task ConnectControllerAsync()
    {
        var code = string.IsNullOrWhiteSpace(ControllerCode) 
            ? (IsQrScanned ? "AQ-ESP32-AUTO" : string.Empty) 
            : ControllerCode.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            await Shell.Current.DisplayAlert("Внимание", "Наведите камеру на QR-код контроллера или введите код вручную", "OK");
            return;
        }

        var name = string.IsNullOrWhiteSpace(ControllerName)
            ? $"Контроллер {(!string.IsNullOrWhiteSpace(code) ? (code.Length > 6 ? code[^4..] : code) : "№1")}"
            : ControllerName.Trim();

        var mac = string.IsNullOrWhiteSpace(MacAddress) ? "00:1A:2B:3C:4D:5E" : MacAddress.Trim();
        var model = string.IsNullOrWhiteSpace(ModelType) ? "ESP32-AquaHub v2" : ModelType;

        // Show Fullscreen connecting state
        IsFormVisible = false;
        IsConnectingVisible = true;

        await Task.Delay(1200);

        var controller = new ControllerModel
        {
            Name = name,
            ModelType = model,
            MacAddress = mac,
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
