using Avalonia;
using Avalonia.ReactiveUI;

namespace FBXViewer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}