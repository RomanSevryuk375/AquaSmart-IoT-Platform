namespace Mobile;

public partial class MainPage : ContentPage
{
    private bool _animationFinished;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_animationFinished) return;

        try
        {
            // Figma Preloader Animation (8-stage sequence)
            // Stage 1-3: Fish enters from left and swims to center
            await PreloaderContainer.TranslateTo(0, 0, 750, Easing.CubicOut);

            // Stage 4: Bubble 1 appears at mouth
            await Task.WhenAll(
                Bubble1.FadeTo(1.0, 150),
                Bubble1.ScaleTo(1.0, 200, Easing.SpringOut)
            );

            // Stage 5: Bubble 2 appears
            await Task.WhenAll(
                Bubble2.FadeTo(1.0, 150),
                Bubble2.ScaleTo(1.0, 200, Easing.SpringOut)
            );

            // Stage 6: Bubble 3 appears
            await Task.WhenAll(
                Bubble3.FadeTo(1.0, 150),
                Bubble3.ScaleTo(1.0, 200, Easing.SpringOut)
            );

            // Brief pause at complete 3-bubble state (matching Figma Frame 6)
            await Task.Delay(350);

            // Stage 7: Fish exits gently right while logo appears
            _ = PreloaderContainer.TranslateTo(260, 0, 500, Easing.CubicIn);
            _ = PreloaderContainer.FadeTo(0, 450);

            // Stage 8: Reveal brand logo & CTA buttons
            await Task.WhenAll(
                LogoImage.FadeTo(1.0, 500, Easing.CubicOut),
                LogoImage.ScaleTo(1.0, 500, Easing.CubicOut)
            );

            await Task.WhenAll(
                ActionsContainer.FadeTo(1.0, 400, Easing.CubicOut),
                ActionsContainer.TranslateTo(0, 0, 400, Easing.CubicOut)
            );

            _animationFinished = true;
        }
        catch
        {
            // Fallback: immediately show logo and buttons
            LogoImage.Opacity = 1;
            LogoImage.Scale = 1;
            ActionsContainer.Opacity = 1;
            ActionsContainer.TranslationY = 0;
            PreloaderContainer.Opacity = 0;
            _animationFinished = true;
        }
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
