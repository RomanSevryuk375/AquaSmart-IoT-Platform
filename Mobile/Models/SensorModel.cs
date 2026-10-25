using CommunityToolkit.Mvvm.ComponentModel;

namespace Mobile.Models;

public partial class SensorModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AquariumId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string PortDisplay => string.IsNullOrWhiteSpace(Port) ? string.Empty : $"Порт: {Port}";
    public bool HasPort => !string.IsNullOrWhiteSpace(Port);
    public SensorType Type { get; set; } = SensorType.Temperature;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAlert))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    private double _value;

    public string Unit { get; set; } = "°C";
    public double MinThreshold { get; set; }
    public double MaxThreshold { get; set; }
    public bool IsAlert => Value < MinThreshold || Value > MaxThreshold;
    public string StatusText => IsAlert ? "Отклонение от нормы" : "Норма";
    public string StatusColor => IsAlert ? "#E53935" : "#388E3C";
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
