using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;


namespace LocationHeatMap;

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
        // Inintialize the MAPs before using
        // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/map?view=net-maui-8.0#map-initialization
        builder.UseMauiMaps();
        builder.UseSkiaSharp();
#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
