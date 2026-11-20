namespace Mobile.Extensions;

public static class AnimationExtensions
{
    /// <summary>
    /// Tactile press animation for buttons, borders and interactive cards.
    /// </summary>
    public static async Task PressAsync(this VisualElement view, double scale = 0.96, uint duration = 75)
    {
        try
        {
            await view.ScaleTo(scale, duration, Easing.CubicOut);
            await view.ScaleTo(1.0, duration, Easing.CubicIn);
        }
        catch
        {
            view.Scale = 1.0;
        }
    }

    /// <summary>
    /// Horizontal shake animation for validation errors (e.g. invalid inputs, empty required fields).
    /// </summary>
    public static async Task ShakeAsync(this VisualElement view, uint stepDuration = 45)
    {
        try
        {
            await view.TranslateTo(-12, 0, stepDuration, Easing.Linear);
            await view.TranslateTo(12, 0, stepDuration, Easing.Linear);
            await view.TranslateTo(-8, 0, stepDuration, Easing.Linear);
            await view.TranslateTo(8, 0, stepDuration, Easing.Linear);
            await view.TranslateTo(-4, 0, stepDuration, Easing.Linear);
            await view.TranslateTo(4, 0, stepDuration, Easing.Linear);
            await view.TranslateTo(0, 0, stepDuration, Easing.Linear);
        }
        catch
        {
            view.TranslationX = 0;
        }
    }

    /// <summary>
    /// Soft pulse animation for alert badges and critical sensor notifications.
    /// </summary>
    public static async Task PulseAsync(this VisualElement view, double scale = 1.08, uint duration = 160)
    {
        try
        {
            await view.ScaleTo(scale, duration, Easing.CubicOut);
            await view.ScaleTo(1.0, duration, Easing.CubicIn);
        }
        catch
        {
            view.Scale = 1.0;
        }
    }

    /// <summary>
    /// Smooth entrance animation with fade-in and slide up.
    /// </summary>
    public static async Task FadeInUpAsync(this VisualElement view, uint duration = 350, double translationY = 24)
    {
        try
        {
            view.Opacity = 0;
            view.TranslationY = translationY;
            await Task.WhenAll(
                view.FadeTo(1.0, duration, Easing.CubicOut),
                view.TranslateTo(0, 0, duration, Easing.CubicOut)
            );
        }
        catch
        {
            view.Opacity = 1.0;
            view.TranslationY = 0;
        }
    }

    /// <summary>
    /// Staggered cascade entrance animation for collections of cards or elements.
    /// </summary>
    public static async Task CascadeEntranceAsync(this IEnumerable<VisualElement> elements, uint duration = 300, int delayMs = 60)
    {
        foreach (var el in elements)
        {
            _ = el.FadeInUpAsync(duration, 20);
            await Task.Delay(delayMs);
        }
    }
}
