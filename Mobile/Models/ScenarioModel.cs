namespace Mobile.Models;

public class ScenarioModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AquariumId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConditionText { get; set; } = string.Empty; // e.g. "Если Температура < 24.0°C"
    public string ActionText { get; set; } = string.Empty;    // e.g. "Включить Нагреватель"
    public bool IsActive { get; set; } = true;
    public string FrequencyText { get; set; } = "Постоянно";
}
