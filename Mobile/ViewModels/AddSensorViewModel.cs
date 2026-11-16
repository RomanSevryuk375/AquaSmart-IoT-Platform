using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

[QueryProperty(nameof(AquariumIdString), "aquariumId")]
public partial class AddSensorViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private string? _aquariumIdString;

    [ObservableProperty]
    private string _sensorName = string.Empty;

    [ObservableProperty]
    private int _sensorTypeIndex = 0; // 0: Temp, 1: pH, 2: O2, 3: TDS

    [ObservableProperty]
    private int _protocolIndex = 0; // 0: Digital, 1: Analog, 2: I2C, 3: OneWire

    [ObservableProperty]
    private string _port = string.Empty;

    [ObservableProperty]
    private string _minValueString = string.Empty;

    [ObservableProperty]
    private string _maxValueString = string.Empty;

    [ObservableProperty]
    private bool _isCancelDialogVisible;

    [ObservableProperty]
    private bool _isSuccessVisible;

    [ObservableProperty]
    private string _createdSensorName = string.Empty;

    [ObservableProperty]
    private string _createdSensorSubtitle = string.Empty;

    [ObservableProperty]
    private string _createdSensorIcon = "🌡️";

    public bool IsFormVisible => !IsSuccessVisible;

    public List<string> SensorTypes => new()
    {
        LocalizationService.Instance["Sensor_Picker_Temp"],
        LocalizationService.Instance["Sensor_Picker_Ph"],
        LocalizationService.Instance["Sensor_Picker_O2"],
        LocalizationService.Instance["Sensor_Picker_Tds"],
        LocalizationService.Instance["Sensor_Picker_Pressure"],
        LocalizationService.Instance["Sensor_Picker_Voltage"]
    };

    public AddSensorViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private void SelectPortPreset(string preset)
    {
        Port = preset;
    }

    [RelayCommand]
    private void PromptCancel()
    {
        IsCancelDialogVisible = true;
    }

    [RelayCommand]
    private void ResumeEditing()
    {
        IsCancelDialogVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmCancelAsync()
    {
        IsCancelDialogVisible = false;
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(SensorName))
        {
            await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Attention"], LocalizationService.Instance["Dialog_FillSensorName"], LocalizationService.Instance["Common_Ok"]);
            return;
        }

        if (string.IsNullOrWhiteSpace(Port))
        {
            await Shell.Current.DisplayAlert(LocalizationService.Instance["Common_Attention"], LocalizationService.Instance["Dialog_FillPort"], LocalizationService.Instance["Common_Ok"]);
            return;
        }

        Guid.TryParse(AquariumIdString, out var aqId);

        var (type, unit, icon, typeName, defMin, defMax) = SensorTypeIndex switch
        {
            1 => (SensorType.Ph, "pH", "🧪", LocalizationService.Instance["SensorType_Ph"], 6.5, 8.0),
            2 => (SensorType.DissolvedOxygen, LocalizationService.Instance.IsEnglish ? "mg/L" : "мг/л", "🫧", LocalizationService.Instance["SensorType_Oxygen"], 5.0, 9.0),
            3 => (SensorType.TdsSalinity, "ppm", "🧂", LocalizationService.Instance["SensorType_Tds"], 100.0, 300.0),
            4 => (SensorType.WaterLevel, "hPa", "🧭", LocalizationService.Instance["SensorType_Pressure"], 950.0, 1050.0),
            5 => (SensorType.Temperature, "V", "⚡", LocalizationService.Instance["SensorType_Voltage"], 3.0, 5.5),
            _ => (SensorType.Temperature, "°C", "🌡️", LocalizationService.Instance["SensorType_Temp"], 22.0, 28.0)
        };

        var min = double.TryParse(MinValueString, System.Globalization.CultureInfo.InvariantCulture, out var parsedMin)
            ? parsedMin
            : (double.TryParse(MinValueString, out parsedMin) ? parsedMin : defMin);

        var max = double.TryParse(MaxValueString, System.Globalization.CultureInfo.InvariantCulture, out var parsedMax)
            ? parsedMax
            : (double.TryParse(MaxValueString, out parsedMax) ? parsedMax : defMax);

        var protocolName = ProtocolIndex switch
        {
            1 => "Analog",
            2 => "I2C",
            3 => "OneWire",
            _ => "Digital"
        };

        var sensor = new SensorModel
        {
            AquariumId = aqId,
            Name = SensorName.Trim(),
            Port = Port.Trim(),
            Type = type,
            Value = (min + max) / 2.0,
            Unit = unit,
            MinThreshold = min,
            MaxThreshold = max
        };

        await _aquariumService.AddSensorAsync(sensor);

        CreatedSensorName = SensorName.Trim();
        CreatedSensorIcon = icon;
        CreatedSensorSubtitle = $"{typeName} • {protocolName} • {Port.Trim()}";
        IsSuccessVisible = true;
        OnPropertyChanged(nameof(IsFormVisible));
    }

    [RelayCommand]
    private async Task ReturnToAquariumAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void AddAnotherSensor()
    {
        SensorName = string.Empty;
        Port = string.Empty;
        IsSuccessVisible = false;
        IsCancelDialogVisible = false;
        OnPropertyChanged(nameof(IsFormVisible));
    }
}
