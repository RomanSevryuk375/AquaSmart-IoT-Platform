namespace Mobile.Models;

public enum AquariumDeviceType
{
    Heater,     // Нагреватель
    Aerator,    // Аэратор (компрессор)
    FilterPump, // Фильтр / помпа
    Feeder,     // Автокормушка
    Lighting    // Освещение
}

public class RelayModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AquariumId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AquariumDeviceType Type { get; set; } = AquariumDeviceType.Aerator;
    public bool IsOn { get; set; }
    public int PinChannel { get; set; } = 1;
    public string ScheduleInfo { get; set; } = "По расписанию";
    public string Icon => Type switch
    {
        AquariumDeviceType.Heater => "🔥",
        AquariumDeviceType.Aerator => "💨",
        AquariumDeviceType.FilterPump => "🔄",
        AquariumDeviceType.Feeder => "🥣",
        AquariumDeviceType.Lighting => "💡",
        _ => "⚡"
    };
    public string StateText => IsOn ? "Включено" : "Выключено";
    public string StateBadgeColor => IsOn ? "#388E3C" : "#757575";
}
