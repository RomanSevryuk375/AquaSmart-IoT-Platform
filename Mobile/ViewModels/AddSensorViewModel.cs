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
    private string _minValueString = "22.0";

    [ObservableProperty]
    private string _maxValueString = "28.0";

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
        double.TryParse(MinValueString, out var min);
        double.TryParse(MaxValueString, out var max);

        var (type, unit, icon, typeName) = SensorTypeIndex switch
        {
            1 => (SensorType.Ph, "pH", "🧪", "Кислотность pH"),
            2 => (SensorType.DissolvedOxygen, "мг/л", "🫧", "Кислород O2"),
            3 => (SensorType.TdsSalinity, "ppm", "🧂", "Минерализация TDS"),
            _ => (SensorType.Temperature, "°C", "🌡️", "Температура воды")
        };

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
