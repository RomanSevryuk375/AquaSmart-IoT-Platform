namespace Mobile.Models;

public class DashboardEventModel
{
    public string TimeText { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsWarning { get; set; }

    public string BorderColor => IsWarning ? "#FFE082" : "#E4E7F4";
    public string BackgroundColor => IsWarning ? "#FFF9E6" : "#FFFFFF";
    public string TimeColor => IsWarning ? "#E65100" : "#8C93A8";
}
