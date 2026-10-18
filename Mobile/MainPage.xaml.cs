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
            await SwimmingFish.TranslateTo(0, 0, 750, Easing.CubicOut);

            // Stage 4-6: Rising Bubbles
            await Task.WhenAll(
                Bubble1.FadeTo(0.9, 200),
                Bubble1.TranslateTo(0, -18, 300, Easing.SinOut)
            );

            await Task.WhenAll(
                Bubble2.FadeTo(0.9, 200),
                Bubble2.TranslateTo(0, -26, 300, Easing.SinOut)
            );

            await Task.WhenAll(
                Bubble3.FadeTo(0.9, 200),
                Bubble3.TranslateTo(0, -32, 300, Easing.SinOut)
            );

            // Stage 7: Fish exits gently right while logo appears
            _ = SwimmingFish.TranslateTo(200, 0, 600, Easing.CubicIn);
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
