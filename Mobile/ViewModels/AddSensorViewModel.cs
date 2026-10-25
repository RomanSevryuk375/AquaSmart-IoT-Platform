using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
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
            await Shell.Current.DisplayAlert("Внимание", "Укажите название датчика", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Port))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите адрес или порт подключения", "OK");
            return;
        }

        Guid.TryParse(AquariumIdString, out var aqId);

        var (type, unit, icon, typeName, defMin, defMax) = SensorTypeIndex switch
        {
            1 => (SensorType.Ph, "pH", "🧪", "Кислотность pH", 6.5, 8.0),
            2 => (SensorType.DissolvedOxygen, "мг/л", "🫧", "Кислород O2", 5.0, 9.0),
            3 => (SensorType.TdsSalinity, "ppm", "🧂", "Минерализация TDS", 100.0, 300.0),
            _ => (SensorType.Temperature, "°C", "🌡️", "Температура воды", 22.0, 28.0)
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
