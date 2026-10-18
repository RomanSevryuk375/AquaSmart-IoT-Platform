namespace Mobile.Views.Controls;

public partial class FishMascotCard : ContentView
{
    public static readonly BindableProperty BadgeTextProperty =
        BindableProperty.Create(nameof(BadgeText), typeof(string), typeof(FishMascotCard), "hi");

    public static readonly BindableProperty BadgeColorProperty =
        BindableProperty.Create(nameof(BadgeColor), typeof(Color), typeof(FishMascotCard), Color.FromArgb("#7BA028"));

    public static readonly BindableProperty HeightOverrideProperty =
        BindableProperty.Create(nameof(HeightOverride), typeof(double), typeof(FishMascotCard), 160.0);

    public string BadgeText
    {
        get => (string)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    public Color BadgeColor
    {
        get => (Color)GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }

    public double HeightOverride
    {
        get => (double)GetValue(HeightOverrideProperty);
        set => SetValue(HeightOverrideProperty, value);
    }

    public FishMascotCard()
    {
        InitializeComponent();
    }
}
