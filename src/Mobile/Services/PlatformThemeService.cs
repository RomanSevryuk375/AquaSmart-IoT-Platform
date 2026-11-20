namespace Mobile.Services;

public static class PlatformThemeService
{
    public static void ApplySystemBarsTheme(AppTheme? theme = null)
    {
#if ANDROID
        try
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity == null || activity.Window == null) return;

            var currentTheme = theme ?? Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
            bool isDark;
            if (currentTheme == AppTheme.Unspecified)
            {
                var uiMode = activity.Resources?.Configuration?.UiMode;
                if (uiMode.HasValue)
                {
                    isDark = (uiMode.Value & Android.Content.Res.UiMode.NightMask) == Android.Content.Res.UiMode.NightYes;
                }
                else
                {
                    isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
                }
            }
            else
            {
                isDark = currentTheme == AppTheme.Dark;
            }

            activity.RunOnUiThread(() =>
            {
                try
                {
                    var window = activity.Window;
                    if (window == null) return;

                    // Ensure window draws system bar backgrounds rather than translucent overlays
                    window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
                    window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentNavigation);
                    window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);

                    // On Android 10 (API 29)+, disable system-enforced scrim contrast so navbar uses our exact dark color
                    if (OperatingSystem.IsAndroidVersionAtLeast(29))
                    {
                        window.NavigationBarContrastEnforced = false;
                        window.StatusBarContrastEnforced = false;
                    }

                    var navBarColor = isDark
                        ? Android.Graphics.Color.ParseColor("#161824")
                        : Android.Graphics.Color.ParseColor("#FFFFFF");

                    var statusBarColor = isDark
                        ? Android.Graphics.Color.ParseColor("#12131A")
                        : Android.Graphics.Color.ParseColor("#FFFFFF");

                    window.SetNavigationBarColor(navBarColor);
                    window.SetStatusBarColor(statusBarColor);

                    var decorView = window.DecorView;
                    if (decorView != null)
                    {
                        var insetsController = AndroidX.Core.View.WindowCompat.GetInsetsController(window, decorView);
                        if (insetsController != null)
                        {
                            // AppearanceLightNavigationBars = false -> buttons are white/light on dark navbar
                            // AppearanceLightNavigationBars = true -> buttons are dark on light navbar
                            insetsController.AppearanceLightNavigationBars = !isDark;
                            insetsController.AppearanceLightStatusBars = !isDark;
                        }

                        // Fallback for older Android / OEM ROMs (MIUI, OneUI)
                        if (!OperatingSystem.IsAndroidVersionAtLeast(30))
                        {
#pragma warning disable CS0618
                            var flags = (int)decorView.SystemUiVisibility;
                            if (OperatingSystem.IsAndroidVersionAtLeast(26))
                            {
                                const int flagLightNav = (int)Android.Views.SystemUiFlags.LightNavigationBar;
                                if (isDark) flags &= ~flagLightNav; else flags |= flagLightNav;
                            }
                            if (OperatingSystem.IsAndroidVersionAtLeast(23))
                            {
                                const int flagLightStatus = (int)Android.Views.SystemUiFlags.LightStatusBar;
                                if (isDark) flags &= ~flagLightStatus; else flags |= flagLightStatus;
                            }
                            decorView.SystemUiVisibility = (Android.Views.StatusBarVisibility)flags;
#pragma warning restore CS0618
                        }
                    }
                }
                catch
                {
                }
            });
        }
        catch
        {
        }
#endif
    }
}
