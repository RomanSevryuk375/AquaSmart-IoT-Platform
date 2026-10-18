namespace Mobile;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Smooth initial entrance animation (Preloader -> Welcome Screen)
        await Task.WhenAll(
            LogoImage.FadeTo(1.0, 650, Easing.CubicOut),
            LogoImage.ScaleTo(1.0, 650, Easing.CubicOut)
        );

        await Task.WhenAll(
            ActionsContainer.FadeTo(1.0, 450, Easing.CubicOut),
            ActionsContainer.TranslateTo(0, 0, 450, Easing.CubicOut)
        );
    }

    private async void OnOnboardingClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            await btn.ScaleTo(0.97, 70);
            await btn.ScaleTo(1.0, 70);
        }

        await Shell.Current.GoToAsync("OnboardingPage");
    }

    private async void OnAuthorizationClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            await btn.ScaleTo(0.97, 70);
            await btn.ScaleTo(1.0, 70);
        }

        await Shell.Current.GoToAsync("LoginPage");
    }
}
