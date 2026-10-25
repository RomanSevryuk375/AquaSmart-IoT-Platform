using Mobile.Services.Interfaces;

namespace Mobile;

public partial class App : Application
{
    private readonly ISecurityService? _securityService;
    private DateTime _lastBackgroundTime = DateTime.UtcNow;
    private bool _isUnlocking;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _securityService = serviceProvider.GetService<ISecurityService>();
        ApplySavedTheme();
        MainPage = serviceProvider.GetRequiredService<AppShell>();
    }

    public App()
    {
        InitializeComponent();
        ApplySavedTheme();
        MainPage = new AppShell();
    }

    public void ApplySavedTheme()
    {
        try
        {
            var savedTheme = Preferences.Default.Get("setting_theme", "Системная");
            UserAppTheme = savedTheme switch
            {
                "Тёмная" => AppTheme.Dark,
                "Светлая" => AppTheme.Light,
                _ => AppTheme.Unspecified
            };
        }
        catch
        {
        }
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        _lastBackgroundTime = DateTime.UtcNow;
    }

    protected override async void OnResume()
    {
        base.OnResume();
        if (_securityService == null || _isUnlocking) return;

        if (_securityService.ShouldAutoLock(_lastBackgroundTime))
        {
            _isUnlocking = true;
            try
            {
                await _securityService.CheckAppUnlockPinAsync();
            }
            finally
            {
                _isUnlocking = false;
                _lastBackgroundTime = DateTime.UtcNow;
            }
        }
    }
}
