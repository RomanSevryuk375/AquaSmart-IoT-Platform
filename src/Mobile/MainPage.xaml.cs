using Mobile.Services.Interfaces;

namespace Mobile;

public partial class MainPage : ContentPage
{
    private readonly IAuthService? _authService;
    private bool _animationFinished;

    public MainPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    public MainPage()
    {
        InitializeComponent();
        _authService = IPlatformApplication.Current?.Services?.GetService<IAuthService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_animationFinished)
        {
            if (_authService != null && await _authService.TryAutoLoginAsync())
            {
                await Shell.Current.GoToAsync("//MainTabs/DashboardPage");
                return;
            }

            LogoImage.Opacity = 1;
            LogoImage.Scale = 1;
            ActionsContainer.Opacity = 1;
            ActionsContainer.TranslationY = 0;
            PreloaderContainer.Opacity = 0;
            return;
        }

        try
        {
            // Start checking authentication in the background while splash plays
            var authCheckTask = _authService != null ? _authService.TryAutoLoginAsync() : Task.FromResult(false);

            // Figma Preloader Animation (8-stage sequence)
            await PreloaderContainer.TranslateTo(0, 0, 650, Easing.CubicOut);

            await Task.WhenAll(
                Bubble1.FadeTo(1.0, 140),
                Bubble1.ScaleTo(1.0, 180, Easing.SpringOut)
            );

            await Task.WhenAll(
                Bubble2.FadeTo(1.0, 140),
                Bubble2.ScaleTo(1.0, 180, Easing.SpringOut)
            );

            await Task.WhenAll(
                Bubble3.FadeTo(1.0, 140),
                Bubble3.ScaleTo(1.0, 180, Easing.SpringOut)
            );

            var isAuthenticated = await authCheckTask;
            if (isAuthenticated)
            {
                _animationFinished = true;
                await Shell.Current.GoToAsync("//MainTabs/DashboardPage");
                return;
            }

            // Brief pause at complete 3-bubble state
            await Task.Delay(200);

            _ = PreloaderContainer.TranslateTo(260, 0, 450, Easing.CubicIn);
            _ = PreloaderContainer.FadeTo(0, 400);

            await Task.WhenAll(
                LogoImage.FadeTo(1.0, 450, Easing.CubicOut),
                LogoImage.ScaleTo(1.0, 450, Easing.CubicOut)
            );

            await Task.WhenAll(
                ActionsContainer.FadeTo(1.0, 350, Easing.CubicOut),
                ActionsContainer.TranslateTo(0, 0, 350, Easing.CubicOut)
            );

            _animationFinished = true;
        }
        catch
        {
            if (_authService != null && await _authService.TryAutoLoginAsync())
            {
                await Shell.Current.GoToAsync("//MainTabs/DashboardPage");
                return;
            }

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
