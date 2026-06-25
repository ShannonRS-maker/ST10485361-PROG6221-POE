using System;
using Avalonia;

namespace Prog_part1
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Application Initialization Failure: {ex.Message}");
            }
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            // CHANGED: We now safely bind to your custom 'App' class instead of 'Application'
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions())
                .With(new X11PlatformOptions())
                .With(new MacOSPlatformOptions
                {
                    ShowInDock = true
                })
                .LogToTrace();
        }
    }
}