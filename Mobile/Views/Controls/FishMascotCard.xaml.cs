namespace Mobile.Views.Controls;

public partial class FishMascotCard : ContentView
{
    public static readonly BindableProperty BadgeIconProperty =
        BindableProperty.Create(
            nameof(BadgeIcon),
            typeof(string),
            typeof(FishMascotCard),
            string.Empty,
            propertyChanged: (bindable, _, _) => ((FishMascotCard)bindable).UpdateBadgeDisplay());

    public static readonly BindableProperty BadgeTextProperty =
        BindableProperty.Create(
            nameof(BadgeText),
            typeof(string),
            typeof(FishMascotCard),
            "hi",
            propertyChanged: (bindable, _, _) => ((FishMascotCard)bindable).UpdateBadgeDisplay());

    public static readonly BindableProperty BadgeColorProperty =
        BindableProperty.Create(
            nameof(BadgeColor),
            typeof(Color),
            typeof(FishMascotCard),
            Color.FromArgb("#7BA028"),
            propertyChanged: (bindable, _, newValue) =>
            {
                if (bindable is FishMascotCard card && newValue is Color col && card.BadgeBorder != null)
                {
                    card.BadgeBorder.BackgroundColor = col;
                }
            });

    public static readonly BindableProperty HeightOverrideProperty =
        BindableProperty.Create(
            nameof(HeightOverride),
            typeof(double),
            typeof(FishMascotCard),
            160.0,
            propertyChanged: (bindable, _, newValue) =>
            {
                if (bindable is FishMascotCard card && newValue is double h && card.CardBorder != null)
                {
                    card.CardBorder.HeightRequest = h;
                }
            });

    public FishMascotCard()
    {
        InitializeComponent();
        UpdateBadgeDisplay();
    }

    public string BadgeIcon
    {
        get => (string)GetValue(BadgeIconProperty);
        set => SetValue(BadgeIconProperty, value);
    }

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

    private void UpdateBadgeDisplay()
    {
        if (BadgeBorder == null || BadgeIconImage == null || BadgeTextLabel == null || CardBorder == null) return;

        BadgeBorder.BackgroundColor = BadgeColor;
        CardBorder.HeightRequest = HeightOverride;

        if (!string.IsNullOrWhiteSpace(BadgeIcon))
        {
            BadgeIconImage.Source = BadgeIcon;
            BadgeIconImage.IsVisible = true;
            BadgeTextLabel.IsVisible = false;
        }
        else
        {
            BadgeIconImage.Source = null;
            BadgeIconImage.IsVisible = false;
            BadgeTextLabel.Text = BadgeText;
            BadgeTextLabel.IsVisible = !string.IsNullOrEmpty(BadgeText);
        }
    }
}
