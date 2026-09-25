using Avalonia;
using ProjectTemplate.Bootstrap;

namespace ProjectTemplate.Desktop;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args) =>
        BuildAvaloniaApp(CompositionRoot.Create()).StartWithClassicDesktopLifetime(args);

    /// <summary>Used by the XAML previewer.</summary>
    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp(CompositionRoot.Create());

    public static AppBuilder BuildAvaloniaApp(AppServices services) =>
        AppBuilder.Configure(() => new App(services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
