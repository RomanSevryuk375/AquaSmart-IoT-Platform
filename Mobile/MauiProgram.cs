using Microsoft.Extensions.Logging;
using Mobile.Extensions;

#if ANDROID || IOS
using ZXing.Net.Maui.Controls;
#endif

namespace Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
#if ANDROID || IOS
            .UseBarcodeReader()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register application services, ViewModels and Views using CarsheringMobile architectural pattern
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddApplicationServices();
        builder.Services.AddViewModels();
        builder.Services.AddViews();

#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            handler.PlatformView.Background = null;
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
        });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
