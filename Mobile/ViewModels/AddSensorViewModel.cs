using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    private string _minValueString = "22.0";

    [ObservableProperty]
    private string _maxValueString = "28.0";

    public AddSensorViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(SensorName))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите название датчика", "OK");
            return;
        }

        Guid.TryParse(AquariumIdString, out var aqId);
        double.TryParse(MinValueString, out var min);
        double.TryParse(MaxValueString, out var max);

        var type = SensorTypeIndex switch
        {
            1 => SensorType.Ph,
            2 => SensorType.DissolvedOxygen,
            3 => SensorType.TdsSalinity,
            _ => SensorType.Temperature
        };

        var unit = type switch
        {
            SensorType.Ph => "pH",
            SensorType.DissolvedOxygen => "мг/л",
            SensorType.TdsSalinity => "ppm",
            _ => "°C"
        };

        var sensor = new SensorModel
        {
            AquariumId = aqId,
            Name = SensorName,
            Type = type,
            Value = (min + max) / 2.0,
            Unit = unit,
            MinThreshold = min,
            MaxThreshold = max
        };

        await _aquariumService.AddSensorAsync(sensor);
        await Shell.Current.DisplayAlert("Успех", $"Датчик «{SensorName}» откалиброван и привязан!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
