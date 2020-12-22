using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

namespace FBXViewer
{
    public static class Program
    {
	    public static AppBuilder BuildAvaloniaApp()
		    => AppBuilder.Configure<App>()
			    .UsePlatformDetect()
			    .LogToDebug()
			    .UseReactiveUI();
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}