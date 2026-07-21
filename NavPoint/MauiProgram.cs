using Microsoft.Extensions.Logging;
using NavPoint.Core.Data;
using NavPoint.Core.ViewModels;

namespace NavPoint;

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

        builder.Services.AddSingleton<ILocationRepository>(_ =>
            new SqliteLocationRepository(Path.Combine(FileSystem.AppDataDirectory, "navpoint.db3")));
        builder.Services.AddSingleton<Locations>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
