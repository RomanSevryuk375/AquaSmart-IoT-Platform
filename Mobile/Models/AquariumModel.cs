namespace Mobile.Models;

public class AquariumModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public double VolumeLiters { get; set; }
    public WaterType WaterType { get; set; } = WaterType.Freshwater;
    public string SpeciesInfo { get; set; } = string.Empty;
    public string Dimensions { get; set; } = string.Empty;

    // Current telemetry snapshot
    public double Temperature { get; set; }
    public double Ph { get; set; }
    public double Oxygen { get; set; }
    public double SalinityTds { get; set; }

    public bool IsNormalStatus { get; set; } = true;
    public string StatusText => IsNormalStatus ? "Все системы в норме" : "Требует внимания";
    public string StatusColor => IsNormalStatus ? "#388E3C" : "#E53935";
    public string WaterTypeDisplay => WaterType switch
    {
        WaterType.Marine => "Морской",
        WaterType.Brackish => "Солоноватоводный",
        _ => "Пресноводный"
    };

    public int SensorsCount { get; set; }
    public int RelaysCount { get; set; }
    public int ScenariosCount { get; set; }

    // Visual and meta properties
    public string Icon { get; set; } = "🐟";
    public string ControllerName { get; set; } = "Без контроллера";
    public string CardThemeColor { get; set; } = "#F0F4FF";
    public string CardBorderColor { get; set; } = "#DCE4F7";
    public bool IsOnline { get; set; }
    public string StatusSummary { get; set; } = "• Не привязан";
    public string StatusDotColor { get; set; } = "#8C93A8";
    public string TemperatureDisplay { get; set; } = "-- °C";
    public string TankTypeSubtitle { get; set; } = "Ёмкость";
}
