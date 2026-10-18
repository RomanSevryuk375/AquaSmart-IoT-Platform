using Microsoft.Extensions.Logging;
using Mobile.Extensions;

namespace Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
