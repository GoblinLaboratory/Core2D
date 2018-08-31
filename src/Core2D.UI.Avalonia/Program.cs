using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Core2D.UI.Avalonia.Windows;

namespace Core2D.UI.Avalonia
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            BuildAvaloniaApp().UseWin32().UseDirect2D1().Start<MainWindow>();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .LogToDebug();
    }
}
