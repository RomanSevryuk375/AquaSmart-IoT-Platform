namespace Mobile.Models;

public class ControllerModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ModelType { get; set; } = "ESP32-AquaHub v2";
    public string MacAddress { get; set; } = "24:6F:28:B4:72:01";
    public string IpAddress { get; set; } = "192.168.1.145";
    public string FirmwareVersion { get; set; } = "v2.4.1 (Latest)";
    public bool IsOnline { get; set; } = true;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public Guid? AttachedAquariumId { get; set; }
    public string AttachedAquariumName { get; set; } = "Биотоп Танганьика №1";

    public string StatusText => IsOnline ? "В сети (Online)" : "Не в сети (Offline)";
    public string StatusColor => IsOnline ? "#388E3C" : "#E53935";
    public string StatusBadgeBg => IsOnline ? "#E8F5E9" : "#FFEBEE";
}
