using CommunityToolkit.Mvvm.ComponentModel;
using Mobile.Services;

namespace Mobile.Models;

public partial class SensorModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AquariumId { get; set; }
    public Guid ControllerId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string PortDisplay => string.IsNullOrWhiteSpace(Port) ? string.Empty : $"{LocalizationService.Instance["Tank_PortPrefix"]}: {Port}";
    public bool HasPort => !string.IsNullOrWhiteSpace(Port);
    public SensorType Type { get; set; } = SensorType.Temperature;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAlert))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    [NotifyPropertyChangedFor(nameof(ValueDisplay))]
    private double _value;

    public string Unit { get; set; } = "°C";
    public double MinThreshold { get; set; }
    public double MaxThreshold { get; set; }
    public bool IsAlert => Value < MinThreshold || Value > MaxThreshold;
    public string StatusText => IsAlert ? LocalizationService.Instance["Sensor_Deviation"] : LocalizationService.Instance["Sensor_Normal"];
    public string StatusColor => IsAlert ? "#E53935" : "#388E3C";
    public string ValueDisplay => $"{Value:0.#} {Unit}".Trim();
    public string Icon => Type switch
    {
        SensorType.Temperature => "🌡️",
        SensorType.Ph => "🧪",
        SensorType.DissolvedOxygen => "🫧",
        SensorType.TdsSalinity => "🧂",
        _ => "💧"
    };

    [ObservableProperty]
    private DateTime _lastUpdated = DateTime.Now;
}
