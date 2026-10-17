namespace Mobile;

public partial class MainPage : ContentPage
{
    private readonly Random _random = new();

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnRefreshClicked(object? sender, EventArgs e)
    {
        double temp = 24.5 + (_random.NextDouble() * 1.5);
        double ph = 7.0 + (_random.NextDouble() * 0.4);
        int waterLevel = _random.Next(90, 98);
        int tds = _random.Next(135, 155);

        TempLabel.Text = $"{temp:F1} °C";
        PhLabel.Text = $"{ph:F2} pH";
        WaterLevelLabel.Text = $"{waterLevel}%";
        TdsLabel.Text = $"{tds} ppm";

        SemanticScreenReader.Announce($"Telemetry refreshed. Temperature is now {TempLabel.Text}");
    }
}
