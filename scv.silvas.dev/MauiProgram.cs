using Microsoft.Extensions.Logging;
using scv.silvas.dev.Shared.Data;

namespace scv.silvas.dev;

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

        builder.Services.AddMauiBlazorWebView();

#if WINDOWS
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "scv-silvas", "scv.db");
#else
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "scv.db");
#endif
        builder.Services.AddScvServices(dbPath);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        app.Services.SeedDatabase();

        return app;
    }
}
