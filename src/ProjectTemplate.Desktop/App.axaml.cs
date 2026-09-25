using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjectTemplate.Bootstrap;
using ProjectTemplate.Desktop.Features.Inspect;
using ProjectTemplate.Desktop.Shell;

namespace ProjectTemplate.Desktop;

public sealed partial class App : global::Avalonia.Application
{
    private readonly AppServices? _services;

    /// <summary>Used by the XAML previewer and headless tests.</summary>
    public App()
    {
    }

    public App(AppServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = CreateMainWindow(_services ?? CompositionRoot.Create());
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static MainWindow CreateMainWindow(AppServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new MainWindow
        {
            DataContext = new MainWindowViewModel(new InspectViewModel(services.InspectFile)),
        };
    }
}
