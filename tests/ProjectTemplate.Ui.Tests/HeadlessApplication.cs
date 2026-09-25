using Avalonia;
using Avalonia.Headless;
using ProjectTemplate.Desktop;

[assembly: AvaloniaTestApplication(typeof(ProjectTemplate.Ui.Tests.HeadlessApplication))]

namespace ProjectTemplate.Ui.Tests;

internal static class HeadlessApplication
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .WithInterFont()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
