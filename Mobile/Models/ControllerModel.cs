namespace Mobile.Models;

public class ControllerModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ModelType { get; set; } = "ESP32-AquaHub v2";
    public string MacAddress { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = "v1.0.0";
    public bool IsOnline { get; set; } = false;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public Guid? AttachedAquariumId { get; set; }
    public string AttachedAquariumName { get; set; } = "Не привязан";

    public string StatusText => IsOnline ? "В сети (Online)" : "Не в сети (Offline)";
    public string StatusColor => IsOnline ? "#388E3C" : "#E53935";
    public string StatusBadgeBg => IsOnline ? "#E8F5E9" : "#FFEBEE";
}
