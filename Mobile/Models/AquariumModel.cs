namespace Mobile.Models;

public class AquariumModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public double VolumeLiters { get; set; }
    public WaterType WaterType { get; set; } = WaterType.Freshwater;
    public string SpeciesInfo { get; set; } = string.Empty;
    public string Dimensions { get; set; } = string.Empty; // e.g. "100x50x50 см"

    // Current telemetry snapshot
    public double Temperature { get; set; } = 25.4;
    public double Ph { get; set; } = 7.2;
    public double Oxygen { get; set; } = 8.1; // мг/л
    public double SalinityTds { get; set; } = 220; // ppm

    public bool IsNormalStatus { get; set; } = true;
    public string StatusText => IsNormalStatus ? "Все системы в норме" : "Требует внимания";
    public string StatusColor => IsNormalStatus ? "#388E3C" : "#E53935";
    public string WaterTypeDisplay => WaterType switch
    {
        WaterType.Marine => "Морской",
        WaterType.Brackish => "Солоноватоводный",
        _ => "Пресноводный"
    };

    public int SensorsCount { get; set; } = 3;
    public int RelaysCount { get; set; } = 4;
    public int ScenariosCount { get; set; } = 3;

    // Figma visual and meta properties
    public string Icon { get; set; } = "🐟";
    public string ControllerName { get; set; } = "Контроллер: №123";
    public string CardThemeColor { get; set; } = "#F0F4FF";
    public string CardBorderColor { get; set; } = "#DCE4F7";
    public bool IsOnline { get; set; } = true;
    public string StatusSummary { get; set; } = "• В сети • Всё в порядке";
    public string StatusDotColor { get; set; } = "#4CAF50";
    public string TemperatureDisplay { get; set; } = "28°C";
    public string TankTypeSubtitle { get; set; } = "Аквариум • 20л";
}
