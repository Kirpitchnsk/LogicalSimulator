using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using LogicSimulator;
using LogicSimulator.Views;

namespace UITestsLogicSimulator {
    public static class AvaloniaApp {
        // DI registrations
        /* public static void RegisterDependencies() =>
             Bootstrapper.Register(AvaloniaLocator.CurrentMutable, AvaloniaLocator.Current);*/

        // stop app and cleanup
        public static void Stop() {
            var app = GetApp();
            if (app is IDisposable disposable) Dispatcher.UIThread.Post(disposable.Dispose);

            Dispatcher.UIThread.Post(() => app.Shutdown());
        }

        public static MainWindow GetMainWindow() => (MainWindow) GetApp().MainWindow;

        public static IClassicDesktopStyleApplicationLifetime GetApp() {
            var app = Application.Current ?? throw new Exception("Приложение не найдено");
            var life = app.ApplicationLifetime ?? throw new Exception("Приложение не найдено");
            return (IClassicDesktopStyleApplicationLifetime) life;
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .UseHeadless(); // Need a package Avalonia.Headless 0.10.18 (уже 0.10.19) from NuGet for this method
    }
}
